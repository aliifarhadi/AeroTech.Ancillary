using System.Buffers.Text;
using System.Text;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api;
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

        return (lab, context, Single(rows, "A05").OfferItemId!, Single(rows, "A15").OfferItemId!);
    }

    private static AncillaryOfferRow Single(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode);

    private static Task<SelectionSnapshotResult> SubmitDirectAsync(V8Lab lab, AncillaryShoppingContext context, string selectionId)
        => lab.SubmitAsync([V8Lab.Direct(selectionId)], context);

    [Fact]
    public async Task T01_UnsignedReferenceIsRefused()
    {
        var (lab, _, _, reference) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Proofs.ReadAsync(TokenTamper.Unsigned(reference), SelectionIdKind.Reference));
    }

    [Fact]
    public async Task T02_UnsignedDirectIdIsRefused()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, context, TokenTamper.Unsigned(direct)));
    }

    [Fact]
    public async Task T03_MutatedRateIsRefused()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, context, TokenTamper.Claim(direct, "rate", 1L)));
    }

    [Fact]
    public async Task T04_MutatedQuantityIsRefused()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, context, TokenTamper.Claim(direct, "q", 2)));
    }

    [Fact]
    public async Task T05_MutatedTravellerIsRefused()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, context, TokenTamper.Claim(direct, "tr", "T2")));
    }

    [Fact]
    public async Task T06_MutatedFlightIsRefused()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, context, TokenTamper.Claim(direct, "cv", "F2")));
    }

    [Fact]
    public async Task T07_MutatedPortionIsRefused()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 1);
        var portion = Assert.Single(await lab.ShopAsync(V8Lab.Context())).OfferItemId!;

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, V8Lab.Context(), TokenTamper.Claim(portion, "cv", "P2")));
    }

    [Fact]
    public async Task T08_MutatedExpiryIsRefused()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(
            InvalidSignature,
            422,
            () => SubmitDirectAsync(lab, context, TokenTamper.Claim(direct, "exp", ShoppingLab.Now.AddDays(1).ToUnixTimeSeconds())));
    }

    [Fact]
    public async Task T09_ReferenceIsNotADirectSelectionId()
    {
        var (lab, context, _, reference) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => SubmitDirectAsync(lab, context, reference));
    }

    [Fact]
    public async Task T10_DirectIdIsNotAnOptionsReference()
    {
        var (lab, _, direct, _) = await IssuedAsync();

        await BusinessAssert.ThrowsAsync(InvalidSignature, 422, () => lab.Proofs.ReadAsync(direct, SelectionIdKind.Reference));
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
        var (lab, context, direct, reference) = await IssuedAsync();

        lab.Clock.Now = (await lab.Codec.ReadAsync(direct, SelectionIdKind.Direct)).Selection!.ExpiresAtUtc;

        await BusinessAssert.ThrowsAsync(Expired, 410, () => SubmitDirectAsync(lab, context, direct));
        await BusinessAssert.ThrowsAsync(Expired, 410, () => lab.Proofs.ReadAsync(reference, SelectionIdKind.Reference));
    }

    [Fact]
    public async Task T16_ExpiredSnapshotIsGoneWhileItsRowStillExists()
    {
        var (lab, context, _, reference) = await IssuedAsync();
        var snapshot = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, context);

        lab.Clock.Now = snapshot.ExpiresAtUtc;

        Assert.Single(lab.Shop.Catalog.Snapshots);
        await BusinessAssert.ThrowsAsync(Expired, 410, () => lab.Resolver.ResolveAsync(snapshot.SelectionId));
    }

    [Fact]
    public async Task T17_DirectIdOfARetiredPricingIsRefusedAtSubmit()
    {
        var (lab, context, direct, _) = await IssuedAsync();

        lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 40m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => SubmitDirectAsync(lab, context, direct));
    }

    [Fact]
    public async Task T18_SnapshotOfARetiredPricingIsRefused()
    {
        var (lab, context, _, reference) = await IssuedAsync();
        var snapshot = await lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, context);

        lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 0m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.Resolver.ResolveAsync(snapshot.SelectionId));
    }

    [Fact]
    public async Task ReferenceCarriesNoOfferIdAndOnlyMatchesItsOwnOffer()
    {
        var (lab, context, _, reference) = await IssuedAsync();
        var payload = reference[2..].Split('.')[1];
        var otherOffer = context with { SourceIdentity = context.SourceIdentity with { TrustedSourceReference = "OFFER-2" } };

        Assert.DoesNotContain("\"oid\"", Encoding.UTF8.GetString(Base64Url.DecodeFromChars(payload)));
        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.SubmitAsync(reference, AncillaryProfile.AssistedTravel, Wheelchair, otherOffer));
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public void OptionsWithoutAnOfferIdAreRefusedByTheValidator()
    {
        var validator = new ApiPriceAncillaryOffersCommandValidator();

        var result = validator.Validate(new ApiPriceAncillaryOffersCommand(null, [V8Lab.Line("r_x", AncillaryProfile.AssistedTravel, Wheelchair)]));

        Assert.Contains(result.Errors, error => error.PropertyName == "OfferId");
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
