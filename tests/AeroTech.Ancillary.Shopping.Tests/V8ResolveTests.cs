using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service;
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
    private const int NotSellable = 16820;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record Basket(V8Lab Lab, AncillaryShoppingContext Context, string Bag, string BagsOfTwo, string Meal, string Wheelchair);

    private static async Task<Basket> BasketAsync()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        lab.Sellable("A11", amount: 0m);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);
        var wheelchair = await lab.SubmitAsync(Row(rows, "A15").Reference!, AncillaryProfile.AssistedTravel, new AssistedTravelSelectionOptions("WCHS", null, null), context);

        return new Basket(lab, context, Row(rows, "A05").SelectionId!, Row(rows, "A01").QuantityOptions![1].SelectionId, Row(rows, "A11").SelectionId!, wheelchair.SelectionId);
    }

    private static AncillaryOfferRow Row(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode);

    [Fact]
    public async Task R01_MixedIdsAreGroupedByTheirFamily()
    {
        var basket = await BasketAsync();

        var answer = ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Bag, basket.BagsOfTwo, basket.Meal, basket.Wheelchair]));

        Assert.Equal([basket.Bag, basket.BagsOfTwo], answer.Baggages.Select(item => item.SelectionId));
        Assert.Equal(basket.Meal, Assert.Single(answer.Meals).SelectionId);
        Assert.Equal(basket.Wheelchair, Assert.Single(answer.AssistedTravel).SelectionId);
        Assert.Empty(answer.Pets);
    }

    [Fact]
    public void R02_RequestHoldsIdsOnly()
    {
        var properties = typeof(ServiceResolveGroupedSelectionsQuery).GetProperties().Select(property => property.Name);

        Assert.Equal(["Ids"], properties);
        Assert.Equal(
            JsonUnmappedMemberHandling.Disallow,
            typeof(ServiceResolveGroupedSelectionsQuery).GetCustomAttribute<JsonUnmappedMemberHandlingAttribute>()!.UnmappedMemberHandling);
    }

    [Fact]
    public async Task R03_ItemCarriesTheRealRatePricingProvisionAndDefinition()
    {
        var basket = await BasketAsync();
        var catalog = basket.Lab.Shop.Catalog;

        var item = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Bag])).Baggages);
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

        var meal = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Meal])).Meals);

        Assert.True(meal.Pricing.PricingRateId > 0);
        Assert.Equal((0m, 0m), (meal.Pricing.UnitAmount, meal.Pricing.TotalAmount));
    }

    [Fact]
    public async Task R05_QuantityTwoIsSignedAndTotalled()
    {
        var basket = await BasketAsync();

        var bag = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.BagsOfTwo])).Baggages);

        Assert.Equal((2, 100m, 200m), (bag.Pricing.Quantity, bag.Pricing.UnitAmount, bag.Pricing.TotalAmount));
    }

    [Fact]
    public async Task R06_TypedItemReturnsTheStoredSelection()
    {
        var basket = await BasketAsync();

        var wheelchair = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Wheelchair])).AssistedTravel);

        Assert.Equal("WCHS", wheelchair.Selected!.Value.GetProperty("assistanceSsrCode").GetString());
    }

    [Fact]
    public async Task R07_DuplicateOrTooManyOrBlankIdsFailTheWholeRequest()
    {
        var basket = await BasketAsync();
        var service = new ResolveGroupedSelectionsService(basket.Lab.Resolver);

        await BusinessAssert.ThrowsAsync(InvalidRequest, 422, () => service.ExecuteAsync([basket.Bag, basket.Bag]));
        await BusinessAssert.ThrowsAsync(InvalidRequest, 422, () => service.ExecuteAsync(Enumerable.Range(0, 51).Select(index => $"d_{index}").ToList()));
        await BusinessAssert.ThrowsAsync(InvalidRequest, 422, () => service.ExecuteAsync([basket.Bag, " "]));
        await BusinessAssert.ThrowsAsync(InvalidRequest, 422, () => service.ExecuteAsync([]));
    }

    [Fact]
    public async Task R08_RepeatedResolveGivesTheSameAnswerAndKeepsTheSnapshot()
    {
        var basket = await BasketAsync();
        string[] ids = [basket.Bag, basket.Wheelchair];

        var first = JsonSerializer.Serialize(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync(ids)), Json);
        var second = JsonSerializer.Serialize(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync(ids)), Json);

        Assert.Equal(first, second);
        Assert.Single(basket.Lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task R09_PortionIsReturnedWithoutInventedFlights()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 1);
        var context = V8Lab.Context([Trip.Flight("F1", 101), Trip.Flight("F2", 102)], [Trip.Portion("P1", 1, Trip.Flight("F1", 101), Trip.Flight("F2", 102))]);
        var id = Assert.Single(await lab.ShopAsync(context)).SelectionId!;

        var bag = Assert.Single(ServiceResolvedSelectionsMapper.Map(await lab.Resolver.ResolveAsync([id])).Baggages);

        Assert.Equal(SelectionScope.PerTravellerPerPortion, bag.SelectionScope);
        Assert.Equal(["P1"], bag.Coverage.PortionRefs);
        Assert.Empty(bag.Coverage.FlightRefs);
    }

    [Fact]
    public async Task R10_OfferHashIsWhatOrderingRecomputesFromItsOwnOffer()
    {
        var basket = await BasketAsync();

        var item = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Bag])).Baggages);

        Assert.Equal(SelectionBindings.Offer(V8Lab.Offer, V8Lab.Version), item.SourceBinding.OfferHash);
        Assert.NotEqual(SelectionBindings.Offer("OFFER-2", V8Lab.Version), item.SourceBinding.OfferHash);
    }

    [Fact]
    public async Task R11_UnitTravellerIsTheOneOrderingChecks()
    {
        var basket = await BasketAsync();

        var item = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Bag])).Baggages);

        Assert.Equal(["T1"], item.Coverage.TravellerRefs);
        Assert.Contains(basket.Context.Travellers, traveller => traveller.TravellerRef == "T1");
    }

    [Fact]
    public async Task R12_SalesHashIsWhatOrderingRecomputesFromItsSeller()
    {
        var basket = await BasketAsync();

        var item = Assert.Single(ServiceResolvedSelectionsMapper.Map(await basket.Lab.Resolver.ResolveAsync([basket.Bag])).Baggages);

        Assert.Equal(SelectionBindings.Sales(basket.Context), item.SourceBinding.SalesHash);
        Assert.NotEqual(SelectionBindings.Sales(V8Lab.Context(pointOfSaleId: ShoppingLab.OtherPos)), item.SourceBinding.SalesHash);
    }

    [Fact]
    public async Task R13_SuspendedPricingRefusesAValidUnexpiredId()
    {
        var basket = await BasketAsync();

        basket.Lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 40m).Suspend(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => basket.Lab.Resolver.ResolveAsync([basket.Bag]));
    }

    [Fact]
    public async Task R14_OneRetiredIdFailsTheWholeBatch()
    {
        var basket = await BasketAsync();

        basket.Lab.Shop.Catalog.Pricings.Single(pricing => pricing.Rates.Single().BaseAmount == 40m).Retire(ShoppingLab.Now);

        await BusinessAssert.ThrowsAsync(NotSellable, 409, () => basket.Lab.Resolver.ResolveAsync([basket.Meal, basket.Bag, basket.Wheelchair]));
    }
}
