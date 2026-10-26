using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection.Service;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V8ResolveTests
{
    private const int InvalidRequest = 16801;
    private const int NotFound = 16819;
    private const int NotSellable = 16820;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record Basket(
        V8Lab Lab,
        AncillaryShoppingContext Context,
        AncillarySelectionLine Bag,
        AncillarySelectionLine BagsOfTwo,
        AncillarySelectionLine Meal,
        AncillarySelectionLine Wheelchair)
    {
        public async Task<string> SubmitAsync(params AncillarySelectionLine[] lines) => (await Lab.SubmitAsync(lines, Context)).SelectionId;

        public async Task<ServiceResolvedSelectionsDto> ResolveAsync(params AncillarySelectionLine[] lines)
            => ServiceResolvedSelectionsMapper.Map(await Lab.Resolver.ResolveAsync(await SubmitAsync(lines)));
    }

    private static async Task<Basket> BasketAsync()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        lab.Sellable("A11", amount: 0m);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);

        return new Basket(
            lab,
            context,
            V8Lab.Direct(Row(rows, "A05").OfferItemId!),
            V8Lab.Direct(Row(rows, "A01").OfferItemId!, 2),
            V8Lab.Direct(Row(rows, "A11").OfferItemId!),
            V8Lab.Line(Row(rows, "A15").OfferItemId!, AncillaryProfile.AssistedTravel, new AssistedTravelSelectionOptions("WCHS", null, null)));
    }

    private static AncillaryOfferRow Row(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode);

    [Fact]
    public async Task R01_MixedIdsAreGroupedByTheirFamily()
    {
        var basket = await BasketAsync();
        var selectionId = await basket.SubmitAsync(basket.Bag, basket.BagsOfTwo, basket.Meal, basket.Wheelchair);
        var answer = ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync(selectionId));

        Assert.Equal([basket.Bag.OfferItemId, basket.BagsOfTwo.OfferItemId], answer.Baggages.Select(item => item.OfferItemId));
        Assert.Equal(basket.Meal.OfferItemId, Assert.Single(answer.Meals).OfferItemId);
        Assert.Equal(basket.Wheelchair.OfferItemId, Assert.Single(answer.AssistedTravel).OfferItemId);
        Assert.Equal(2, answer.Baggages[1].Selected.Quantity);
        Assert.Empty(answer.Pets);
    }

    [Fact]
    public void R02_RequestHoldsTheSelectionIdOnly()
    {
        var properties = typeof(ServiceResolveAncillarySelectionQuery).GetProperties().Select(property => property.Name);

        Assert.Equal(["SelectionId"], properties);
        Assert.Equal(
            JsonUnmappedMemberHandling.Disallow,
            typeof(ServiceResolveAncillarySelectionQuery).GetCustomAttribute<JsonUnmappedMemberHandlingAttribute>()!.UnmappedMemberHandling);
    }

    [Fact]
    public async Task R03_ItemCarriesTheRealRatePricingProvisionAndDefinition()
    {
        var basket = await BasketAsync();
        var catalog = basket.Lab.Shop.Catalog;

        var item = Assert.Single((await basket.ResolveAsync(basket.Bag)).Baggages);
        var pricing = catalog.Pricings.Single(row => row.Id == item.Pricing.PricingId);

        Assert.Equal(pricing.Rates.Single().Id, item.Pricing.PricingRateId);
        Assert.Equal(pricing.AncillaryProvisionId, item.Provision.ProvisionId);
        Assert.Equal(catalog.Provisions.Single(row => row.Id == item.Provision.ProvisionId).ServiceDefinitionId, item.Definition.DefinitionVersionId);
        Assert.Equal("A05", item.Definition.VariantCode);
    }

    [Fact]
    public async Task R04_ZeroPriceKeepsItsRealRate()
    {
        var basket = await BasketAsync();

        var meal = Assert.Single((await basket.ResolveAsync(basket.Meal)).Meals);

        Assert.True(meal.Pricing.PricingRateId > 0);
        Assert.Equal((0m, 0m), (meal.Pricing.UnitAmount, meal.Pricing.TotalAmount));
    }

    [Fact]
    public async Task R05_QuantityTwoIsSignedAndTotalled()
    {
        var basket = await BasketAsync();

        var bag = Assert.Single((await basket.ResolveAsync(basket.BagsOfTwo)).Baggages);

        Assert.Equal((2, 100m, 200m), (bag.Pricing.Quantity, bag.Pricing.UnitAmount, bag.Pricing.TotalAmount));
    }

    [Fact]
    public async Task R06_TypedItemReturnsTheStoredSelection()
    {
        var basket = await BasketAsync();

        var wheelchair = Assert.Single((await basket.ResolveAsync(basket.Wheelchair)).AssistedTravel);

        Assert.Equal(1, wheelchair.Selected.Quantity);
        Assert.Equal("WCHS", wheelchair.Selected.Options!.AssistedTravel!["assistanceSsrCode"]!.GetValue<string>());
    }

    [Fact]
    public async Task R07_BlankOrUnknownSelectionIdIsRefused()
    {
        var basket = await BasketAsync();
        var service = new ResolveAncillarySelectionService(basket.Lab.Resolver);

        await BusinessAssert.ThrowsAsync(InvalidRequest, 422, () => service.ExecuteAsync(null));
        await BusinessAssert.ThrowsAsync(InvalidRequest, 422, () => service.ExecuteAsync(" "));
        await BusinessAssert.ThrowsAsync(NotFound, 404, () => service.ExecuteAsync("s_unknown"));
    }

    [Fact]
    public async Task R08_RepeatedResolveGivesTheSameAnswerAndKeepsTheSnapshot()
    {
        var basket = await BasketAsync();
        var selectionId = await basket.SubmitAsync(basket.Bag, basket.Wheelchair);

        var first = JsonSerializer.Serialize(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync(selectionId)), Json);
        var second = JsonSerializer.Serialize(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync(selectionId)), Json);

        Assert.Equal(first, second);
        Assert.Single(basket.Lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task R09_PortionIsReturnedWithoutInventedFlights()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 1);
        var context = V8Lab.Context([Trip.Flight("F1", 101), Trip.Flight("F2", 102)], [Trip.Portion("P1", 1, Trip.Flight("F1", 101), Trip.Flight("F2", 102))]);
        var id = Assert.Single(await lab.ShopAsync(context)).OfferItemId!;
        var selectionId = (await lab.SubmitAsync([V8Lab.Direct(id)], context)).SelectionId;

        var bag = Assert.Single(ServiceResolvedSelectionsMapper.Map(await lab.Resolver.ResolveAsync(selectionId)).Baggages);

        Assert.Equal(SelectionScope.PerTravellerPerPortion, bag.SelectionScope);
        Assert.Equal(["P1"], bag.Coverage.PortionRefs);
        Assert.Empty(bag.Coverage.FlightRefs);
    }

    [Fact]
    public async Task R10_OfferHashIsWhatOrderingRecomputesFromItsOwnOffer()
    {
        var basket = await BasketAsync();

        var item = Assert.Single((await basket.ResolveAsync(basket.Bag)).Baggages);

        Assert.Equal(SelectionBindings.Offer(V8Lab.Offer, V8Lab.Version), item.SourceBinding.OfferHash);
        Assert.NotEqual(SelectionBindings.Offer("OFFER-2", V8Lab.Version), item.SourceBinding.OfferHash);
    }

    [Fact]
    public async Task R11_UnitTravellerIsTheOneOrderingChecks()
    {
        var basket = await BasketAsync();

        var item = Assert.Single((await basket.ResolveAsync(basket.Bag)).Baggages);

        Assert.Equal(["T1"], item.Coverage.TravellerRefs);
        Assert.Contains(basket.Context.Travellers, traveller => traveller.TravellerRef == "T1");
    }

    [Fact]
    public async Task R12_SalesHashIsWhatOrderingRecomputesFromItsSeller()
    {
        var basket = await BasketAsync();

        var item = Assert.Single((await basket.ResolveAsync(basket.Bag)).Baggages);

        Assert.Equal(SelectionBindings.Sales(basket.Context), item.SourceBinding.SalesHash);
        Assert.NotEqual(SelectionBindings.Sales(V8Lab.Context(pointOfSaleId: ShoppingLab.OtherPos)), item.SourceBinding.SalesHash);
    }

    [Fact]
    public async Task R13_SuspendedPricingRefusesAValidUnexpiredId()
    {
        var basket = await BasketAsync();
        var selectionId = await basket.SubmitAsync(basket.Bag);

        basket.Lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 40m).Suspend(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => basket.Lab.Resolver.ResolveAsync(selectionId));
    }

    [Fact]
    public async Task R14_OneRetiredLineFailsTheWholeSelection()
    {
        var basket = await BasketAsync();
        var selectionId = await basket.SubmitAsync(basket.Meal, basket.Bag, basket.Wheelchair);

        basket.Lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 40m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => basket.Lab.Resolver.ResolveAsync(selectionId));
    }
}
