using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V9GroupingTests
{
    private static readonly ShoppingFlight F1 = Trip.Flight("F1", 101);
    private static readonly ShoppingFlight F2 = Trip.Flight("F2", 102);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private static AncillaryShoppingContext TwoTravellersTwoFlights()
        => V8Lab.Context([F1, F2], [Trip.Portion("P1", 1, F1, F2)], [Trip.Adult("T1"), Trip.Adult("T2")]);

    private static AncillaryOffersDto Map(IReadOnlyList<AncillaryOfferRow> rows) => AncillaryOffersMapper.Map(V8Lab.Snapshot(rows));

    private static JsonObject Node<TValue>(TValue value) => JsonNode.Parse(JsonSerializer.Serialize(value, Json))!.AsObject();

    private static IEnumerable<AncillaryUnitOfferDto> Units<TDetails>(IEnumerable<AncillaryVariantGroupDto<TDetails>> cards)
        => cards.SelectMany(card => card.Products).SelectMany(product => product.Units);

    private static string Fingerprint(AncillaryOfferRow row)
        => string.Join('|',
            row.SelectionScope,
            row.OfferItemId,
            row.Options is not null,
            row.UnitAmount,
            $"{row.QuantityPolicy.Selectable}:{row.QuantityPolicy.Min}-{row.QuantityPolicy.Max}",
            string.Join(',', row.Candidate.Scope.TravellerRefs),
            string.Join(',', row.Candidate.Scope.PortionRefs),
            string.Join(',', row.Candidate.Scope.FlightRefs));

    private static string Fingerprint(AncillaryUnitOfferDto unit)
        => string.Join('|',
            unit.SelectionScope,
            unit.OfferItemId,
            unit.Options is not null,
            unit.UnitPrice.Amount,
            $"{unit.QuantityPolicy.Selectable}:{unit.QuantityPolicy.Min}-{unit.QuantityPolicy.Max}",
            string.Join(',', unit.Coverage.TravellerRefs),
            string.Join(',', unit.Coverage.PortionRefs),
            string.Join(',', unit.Coverage.FlightRefs));

    [Fact]
    public async Task G01_G17_EachVariantIsOneCardInFirstAppearanceOrder()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", maxQuantity: 2);

        var rows = await lab.ShopAsync(TwoTravellersTwoFlights());
        var cards = Map(rows).Baggages;

        Assert.Equal(2, cards.Count);
        Assert.Equal(rows.Select(row => row.Candidate.VariantCode).Distinct(), cards.Select(card => card.VariantCode));
        Assert.All(cards, card => Assert.Equal(
            rows.Where(row => row.Candidate.VariantCode == card.VariantCode).Select(Fingerprint),
            card.Products.SelectMany(product => product.Units).Select(Fingerprint)));
    }

    [Fact]
    public async Task G02_TwoTravellersOnTwoFlightsAreFourUnitsOfOneProduct()
    {
        var lab = new V8Lab();
        lab.Sellable("A05");

        var rows = await lab.ShopAsync(TwoTravellersTwoFlights());
        var product = Assert.Single(Assert.Single(Map(rows).Baggages).Products);

        Assert.Equal(4, product.Units.Count);
        Assert.Equal(4, product.Units.Select(unit => unit.OfferItemId).Distinct().Count());
        Assert.Equal(rows.Select(row => row.OfferItemId), product.Units.Select(unit => unit.OfferItemId));
    }

    [Fact]
    public async Task G03_G13_TwoDefinitionsOfOneVariantAreTwoProductsWithTheirOwnDetails()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 2);
        var heavy = lab.Shop.Definition(
            "A01",
            reference: "XBAG_32KG",
            specification: spec => spec with { Baggage = spec.Baggage! with { MaxKgPerPiece = 32m } },
            confirmation: ConfirmationRequirement.Immediate);
        lab.Shop.Price(heavy, lab.Shop.Provision(heavy, disposition: CommercialDisposition.Paid, maxQuantity: 2), ShoppingLab.Rate(140m));

        var rows = await lab.ShopAsync(V8Lab.Context());
        var card = Assert.Single(Map(rows).Baggages);

        Assert.Equal(2, card.Products.Count);
        Assert.Equal(2, card.Products.Select(product => product.ServiceDefinitionRef).Distinct().Count());
        Assert.All(card.Products, product => Assert.Equal(
            lab.Shop.Catalog.Definitions.Single(definition => definition.ServiceDefinitionRef == product.ServiceDefinitionRef).Baggage!.MaxKgPerPiece,
            product.Details.MaxKgPerPiece));
    }

    [Fact]
    public async Task G04_OneDefinitionWithTwoProvisionsIsTwoProducts()
    {
        var lab = new V8Lab();
        var (definition, first, _) = lab.Sellable("A05", amount: 40m);
        var second = lab.Shop.Provision(definition, sequence: 20, disposition: CommercialDisposition.Paid, pos: ShoppingLab.OtherPos);
        lab.Shop.Price(definition, second, ShoppingLab.Rate(45m));

        var rows = await lab.ShopAsync(V8Lab.Context(), V8Lab.Context(pointOfSaleId: ShoppingLab.OtherPos));
        var card = Assert.Single(Map(rows).Baggages);

        Assert.Equal([$"{definition.Id}:{first.Id}", $"{definition.Id}:{second.Id}"], card.Products.Select(product => product.ProductKey));
        Assert.Equal([40m, 45m], card.Products.Select(product => Assert.Single(product.Units).UnitPrice.Amount));
    }

    [Fact]
    public async Task G05_TheSameUnitSignedTwiceStaysTwoUnits()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context()));

        var product = Assert.Single(Assert.Single(Map([row, row with { OfferItemId = "d_TWIN" }]).Baggages).Products);

        Assert.Equal([row.OfferItemId, "d_TWIN"], product.Units.Select(unit => unit.OfferItemId));
        Assert.All(product.Units, unit => Assert.Equal(40m, unit.UnitPrice.Amount));
    }

    [Fact]
    public async Task G06_APortionOfTwoFlightsIsOneUnitCoveringBoth()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 2);

        var rows = await lab.ShopAsync(V8Lab.Context([F1, F2], [Trip.Portion("P1", 1, F1, F2)]));
        var unit = Assert.Single(Units(Map(rows).Baggages));

        Assert.Equal(new TravellerPortionUnitDto("T1", "P1"), unit.Unit);
        Assert.Equal(["F1", "F2"], unit.Coverage.FlightRefs);
    }

    [Fact]
    public async Task G07_AnOrderScopeIsOneOrderUnit()
    {
        var lab = new V8Lab();
        lab.Sellable("A24", specification: spec => spec with { Connectivity = spec.Connectivity! with { MaxDevices = null } }, scope: ServiceCoverageScope.Order);

        var rows = await lab.ShopAsync(TwoTravellersTwoFlights());
        var unit = Assert.Single(Units(Map(rows).Connectivity));

        Assert.Equal(SelectionScope.PerOrder, unit.SelectionScope);
        Assert.IsType<OrderUnitDto>(unit.Unit);
    }

    [Fact]
    public async Task G08_G09_G10_G16_FlattenedUnitsAreExactlyTheRows()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", maxQuantity: 2);
        lab.Sellable("A15", amount: 0m);

        var rows = await lab.ShopAsync(TwoTravellersTwoFlights());
        var answer = Map(rows);
        var units = Units(answer.Baggages).Concat(Units(answer.AssistedTravel)).ToList();

        Assert.Equal(rows.Count, units.Count);
        Assert.Equal(rows.Select(Fingerprint).Order(StringComparer.Ordinal), units.Select(Fingerprint).Order(StringComparer.Ordinal));
        Assert.Contains(units, unit => unit.OfferItemId.StartsWith("d_", StringComparison.Ordinal));
        Assert.Contains(units, unit => unit.QuantityPolicy.Selectable);
        Assert.Contains(units, unit => unit.Options is not null);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task G11_EmptyAndGatedFamiliesAreEmptyArrays()
    {
        var lab = new V8Lab();
        lab.Sellable("A09");

        var answer = Node(Map(await lab.ShopAsync(V8Lab.Context())));

        Assert.All(
            ["baggages", "meals", "pets", "assistedTravel", "airportServices", "priorityServices", "connectivity", "upgrades", "extraSeats", "travelInsurance"],
            family => Assert.Empty(answer[family]!.AsArray()));
    }

    [Fact]
    public async Task G12_TheCardIsNamedByTheVariantAndTheProductByItsCommercialName()
    {
        var lab = new V8Lab();
        lab.Sellable("A15", amount: 0m);

        var rows = await lab.ShopAsync(V8Lab.Context());
        var card = Assert.Single(Map(rows).AssistedTravel);
        var product = Assert.Single(card.Products);

        Assert.Equal(AncillaryVariant.Find("A15")!.Name, card.Title);
        Assert.Equal(rows[0].Candidate.CommercialName, product.Title);
        Assert.Equal(rows[0].Candidate.ServiceDefinitionRef, product.ServiceDefinitionRef);
    }

    [Fact]
    public async Task G14_BackofficeKeepsEnumObjects()
    {
        var lab = new V8Lab();
        lab.Sellable("A01", maxQuantity: 2);

        var card = Node(BackofficeAncillaryOffersMapper.Map(V8Lab.Snapshot(await lab.ShopAsync(V8Lab.Context()))))["baggages"]![0]!.AsObject();
        var product = card["products"]![0]!.AsObject();
        var unit = product["units"]![0]!.AsObject();

        Assert.Equal(["variantCode", "title", "products"], card.Select(member => member.Key));
        Assert.Equal(["productKey", "serviceDefinitionRef", "title", "details", "units"], product.Select(member => member.Key));
        Assert.Equal(["selectionScope", "unit", "coverage", "unitPrice", "quantityPolicy", "offerItemId"], unit.Select(member => member.Key));
        Assert.Equal("PerTravellerPerPortion", (string?)unit["selectionScope"]!["name"]);
        Assert.Equal("Piece", (string?)unit["quantityPolicy"]!["unit"]!["name"]);
        Assert.IsType<JsonObject>(product["details"]!["chargeKind"]);
    }

    [Fact]
    public async Task G15_OtaPanelAndApiUseEnumNames()
    {
        var lab = new V8Lab();
        lab.Sellable("A15", amount: 0m);

        var unit = Node(Map(await lab.ShopAsync(V8Lab.Context())))["assistedTravel"]![0]!["products"]![0]!["units"]![0]!.AsObject();

        Assert.Equal(["selectionScope", "unit", "coverage", "unitPrice", "quantityPolicy", "offerItemId", "options"], unit.Select(member => member.Key));
        Assert.Equal("PerTravellerPerFlight", (string?)unit["selectionScope"]);
        Assert.Equal(JsonValueKind.String, unit["quantityPolicy"]!["unit"]!.GetValueKind());
    }
}
