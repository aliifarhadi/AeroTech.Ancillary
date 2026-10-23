using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V8SecurityTests
{
    private const int InvalidSignature = 16816;
    private const int Expired = 16817;
    private const int BindingMismatch = 16818;
    private const int NotSellable = 16820;

    private static readonly AssistedTravelSelectionOptions Wheelchair = new("WCHR", null, null);

    private static async Task<(V8Lab Lab, AncillaryShoppingContext Context, string Direct, string Reference)> IssuedAsync()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);

        return (lab, context, Single(rows, "A05").SelectionId!, Single(rows, "A15").Reference!);
    }

    private static AncillaryOfferRow Single(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode);

    [Fact]
    public async Task T01_UnsignedReferenceIsRefused()
    {
        var (lab, _, _, reference) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Typed.ReadReferenceAsync(TokenTamper.Unsigned(reference)));
    }

    [Fact]
    public async Task T02_UnsignedDirectIdIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([TokenTamper.Unsigned(direct)]));
    }

    [Fact]
    public async Task T03_MutatedRateIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([TokenTamper.Claim(direct, "rate", 1L)]));
    }

    [Fact]
    public async Task T04_MutatedQuantityIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([TokenTamper.Claim(direct, "q", 2)]));
    }

    [Fact]
    public async Task T05_MutatedTravellerIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([TokenTamper.Claim(direct, "tr", "T2")]));
    }

    [Fact]
    public async Task T06_MutatedFlightIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([TokenTamper.Claim(direct, "cv", "F2")]));
    }

    [Fact]
    public async Task T07_MutatedPortionIsRefused()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 1);
        var portion = Assert.Single(await lab.ShopAsync(V8Lab.Context())).SelectionId!;

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([TokenTamper.Claim(portion, "cv", "P2")]));
    }

    [Fact]
    public async Task T08_MutatedExpiryIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(
            InvalidSignature,
            422,
            () => lab.Resolver.ResolveAsync([TokenTamper.Claim(direct, "exp", ShoppingLab.Now.AddDays(1).ToUnixTimeSeconds())]));
    }

    [Fact]
    public async Task T09_ReferenceIsNotAFinalId()
    {
        var (lab, _, _, reference) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Resolver.ResolveAsync([reference]));
    }

    [Fact]
    public async Task T10_DirectIdIsNotAnOptionsReference()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Typed.ReadReferenceAsync(direct));
    }

    [Fact]
    public async Task T11_OtherSalesContextIsRefusedAtPost()
    {
        var (lab, _, _, reference) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(
            BindingMismatch,
            403,
            () => lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, V8Lab.Context(pointOfSaleId: ShoppingLab.OtherPos)));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task T12_ChildRateIsNeverSignedForAnAdult()
    {
        var lab = new V8Lab();
        lab.Sellable("A15", amount: 0m, passengerType: PassengerTypeCode.CHD);

        var rows = await lab.ShopAsync(V8Lab.Context(travellers: [Trip.Adult("T1"), Trip.Child("T2")]));

        var row = Assert.Single(rows);
        Assert.Equal("T2", row.Candidate.CandidateIdentity.TravellerRef);
    }

    [Fact]
    public async Task T13_RateOfAnotherPointOfSaleIsNotSigned()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", pos: ShoppingLab.OtherPos);

        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
    }

    [Fact]
    public async Task T14_RetiredRateGetsNoNewSelection()
    {
        var lab = new V8Lab();
        var (_, _, pricing) = lab.Sellable("A05");

        pricing.Retire(ShoppingLab.Now);

        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
    }

    [Fact]
    public async Task T15_ExpiredDirectAndReferenceAreGone()
    {
        var (lab, _, direct, reference) = await IssuedAsync();

        lab.Clock.Now = (await lab.Codec.ReadAsync(direct, SelectionIdKind.Direct)).Selection!.ExpiresAtUtc;

        await BusinessAssert.ThrowsAsync(Expired, 410, () => lab.Resolver.ResolveAsync([direct]));
        await BusinessAssert.ThrowsAsync(Expired, 410, () => lab.Typed.ReadReferenceAsync(reference));
    }

    [Fact]
    public async Task T16_ExpiredSnapshotIsGoneWhileItsRowStillExists()
    {
        var (lab, context, _, reference) = await IssuedAsync();
        var snapshot = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, context);

        lab.Clock.Now = snapshot.ExpiresAtUtc;

        Assert.Single(lab.Shop.Catalog.Snapshots);
        await BusinessAssert.ThrowsAsync(Expired, 410, () => lab.Resolver.ResolveAsync([snapshot.SelectionId]));
    }

    [Fact]
    public async Task T17_DirectIdOfARetiredPricingIsRefused()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 40m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.Resolver.ResolveAsync([direct]));
    }

    [Fact]
    public async Task T18_SnapshotOfARetiredPricingIsRefused()
    {
        var (lab, context, _, reference) = await IssuedAsync();
        var snapshot = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, context);

        lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 0m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.Resolver.ResolveAsync([snapshot.SelectionId]));
    }

    [Fact]
    public async Task T19_ReferenceOfASuspendedPricingMakesNoSnapshot()
    {
        var (lab, context, _, reference) = await IssuedAsync();

        lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 0m).Suspend(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, context));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }
}
