using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V9SelectedOffersTests
{
    private const int Expired = 16817;
    private const int BindingMismatch = 16818;
    private const int NotFound = 16819;
    private const int NotSellable = 16820;

    private sealed record Selection(V8Lab Lab, AncillaryShoppingContext Context, string SelectionId);

    private static async Task<Selection> SubmittedAsync()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);
        AncillaryOfferRow Row(string variant) => rows.Single(row => row.Candidate.VariantCode == variant);
        var result = await lab.SubmitAsync(
            [
                V8Lab.Direct(Row("A01").OfferItemId!, 2),
                V8Lab.Line(Row("A15").OfferItemId!, AncillaryProfile.AssistedTravel, new AssistedTravelSelectionOptions("WCHS", null, null))
            ],
            context);

        return new Selection(lab, context, result.SelectionId);
    }

    private static Task<AncillaryOffersDto> GetAsync(Selection selection, params AncillaryShoppingContext[] contexts)
        => new ApiGetPricedAncillaryOffersQueryHandler(new LabSalesScopes(), selection.Lab.Selected(new LabShoppingSource(contexts.Length == 0 ? [selection.Context] : contexts)))
            .Handle(new ApiGetPricedAncillaryOffersQuery(selection.SelectionId), CancellationToken.None);

    private static AncillaryUnitOfferDto Unit<TDetails>(IReadOnlyList<AncillaryVariantGroupDto<TDetails>> family, string variantCode)
        => family.Single(variant => variant.VariantCode == variantCode).Products.Single().Units.Single();

    private static string WithoutSelected(object answer)
    {
        var node = JsonNode.Parse(JsonSerializer.Serialize(answer, HostJson.Options))!;

        foreach (var unit in node.AsObject().Select(member => member.Value).OfType<JsonArray>()
                     .SelectMany(variants => variants)
                     .SelectMany(variant => variant!["products"]!.AsArray())
                     .SelectMany(product => product!["units"]!.AsArray()))
            unit!.AsObject().Remove("selected");

        return node.ToJsonString();
    }

    [Fact]
    public async Task TheAnswerShowsTheSelectedUnitsWithTheirQuantityAndOptions()
    {
        var selection = await SubmittedAsync();

        var answer = await GetAsync(selection);
        var bags = Unit(answer.Baggages, "A01");
        var wheelchair = Unit(answer.AssistedTravel, "A15");

        Assert.Equal(2, bags.Selected!.Quantity);
        Assert.Null(bags.Selected.Options);
        Assert.Equal(1, wheelchair.Selected!.Quantity);
        Assert.Equal("WCHS", wheelchair.Selected.Options!.AssistedTravel!["assistanceSsrCode"]!.GetValue<string>());
        Assert.Null(Unit(answer.Baggages, "A05").Selected);
        Assert.Contains(
            "\"selected\":{\"quantity\":1,\"options\":{\"assistedTravel\":{\"assistanceSsrCode\":\"WCHS\"}}}",
            JsonSerializer.Serialize(wheelchair, HostJson.Options));
    }

    [Fact]
    public async Task TheAnswerIsTheAncillaryOffersAnswerPlusTheSelection()
    {
        var selection = await SubmittedAsync();

        var offers = AncillaryOffersMapper.Map(V8Lab.Snapshot(await selection.Lab.ShopAsync(selection.Context)));
        var selected = await GetAsync(selection);

        Assert.Equal(WithoutSelected(offers), WithoutSelected(selected));
        Assert.NotEqual(JsonSerializer.Serialize(offers, HostJson.Options), JsonSerializer.Serialize(selected, HostJson.Options));
    }

    [Fact]
    public async Task BackofficeShowsTheSameSelection()
    {
        var selection = await SubmittedAsync();

        var answer = await new BackofficeGetPricedAncillaryOffersQueryHandler(new LabSalesScopes(), selection.Lab.Selected(new LabShoppingSource([selection.Context])))
            .Handle(new BackofficeGetPricedAncillaryOffersQuery(selection.SelectionId), CancellationToken.None);

        var wheelchair = answer.AssistedTravel.Single().Products.Single().Units.Single();
        Assert.Equal("WCHS", wheelchair.Selected!.Options!.AssistedTravel!["assistanceSsrCode"]!.GetValue<string>());
    }

    [Fact]
    public async Task ALineThatIsNoLongerOfferedRefusesTheWholeAnswer()
    {
        var selection = await SubmittedAsync();

        selection.Lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 100m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => GetAsync(selection));
    }

    [Fact]
    public async Task AnotherSalesScopeOrAnotherOfferIsRefused()
    {
        var selection = await SubmittedAsync();
        var otherOffer = selection.Context with { SourceIdentity = selection.Context.SourceIdentity with { TrustedSourceReference = "OFFER-2" } };

        await BusinessAssert.ThrowsAsync(BindingMismatch, 403, () => GetAsync(selection, V8Lab.Context(pointOfSaleId: ShoppingLab.OtherPos)));
        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => GetAsync(selection, otherOffer));
    }

    [Fact]
    public async Task AnUnknownOrExpiredSelectionIdIsRefused()
    {
        var selection = await SubmittedAsync();

        await BusinessAssert.ThrowsAsync(NotFound, 404, () => GetAsync(selection with { SelectionId = "s_unknown" }));

        selection.Lab.Clock.Now = Assert.Single(selection.Lab.Shop.Catalog.Snapshots).ExpiresAtUtc;

        await BusinessAssert.ThrowsAsync(Expired, 410, () => GetAsync(selection));
    }
}
