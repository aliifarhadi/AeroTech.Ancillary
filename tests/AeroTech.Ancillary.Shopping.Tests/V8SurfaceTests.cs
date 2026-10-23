using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V8SurfaceTests
{
    private static readonly string[] Families =
        ["baggages", "meals", "pets", "assistedTravel", "airportServices", "priorityServices", "connectivity", "upgrades", "extraSeats", "travelInsurance"];

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private static async Task<(V8Lab Lab, IReadOnlyList<AncillaryOfferRow> Rows)> MixedAsync()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", maxQuantity: 2);
        lab.Sellable("A15", amount: 0m);
        lab.Sellable("A11", amount: 0m);

        return (lab, await lab.ShopAsync(V8Lab.Context()));
    }

    private static JsonObject Node<TValue>(TValue value) => JsonNode.Parse(JsonSerializer.Serialize(value, Json))!.AsObject();

    private static JsonObject Row(JsonObject answer, string family, string variantCode)
        => answer[family]!.AsArray().Select(row => row!.AsObject()).Single(row => (string?)row["variantCode"] == variantCode);

    [Fact]
    public async Task H01_BackofficeAnswersFamilyArraysWithEnumObjects()
    {
        var (_, rows) = await MixedAsync();

        var answer = Node(BackofficeAncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Backoffice"));
        var typed = Row(answer, "assistedTravel", "A15");

        Assert.All(Families, family => Assert.IsType<JsonArray>(answer[family]));
        Assert.Equal("PerTravellerPerFlight", (string?)typed["selectionScope"]!["name"]);
        Assert.StartsWith("/Backoffice/v1/AncillaryOffers/AssistedTravel/r_", (string?)typed["optionsEndpoint"]);
    }

    [Fact]
    public async Task H02_OtaPanelAnswersFamilyArrays()
    {
        var (_, rows) = await MixedAsync();

        var answer = Node(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "OtaPanel"));

        Assert.All(Families, family => Assert.IsType<JsonArray>(answer[family]));
        Assert.Equal("PerTravellerPerFlight", (string?)Row(answer, "assistedTravel", "A15")["selectionScope"]);
        Assert.StartsWith("/OtaPanel/v1/AncillaryOffers/AssistedTravel/r_", (string?)Row(answer, "assistedTravel", "A15")["optionsEndpoint"]);
    }

    [Fact]
    public async Task H03_ApiAnswersFamilyArrays()
    {
        var (_, rows) = await MixedAsync();

        var answer = Node(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api"));

        Assert.All(Families, family => Assert.IsType<JsonArray>(answer[family]));
        Assert.StartsWith("/Api/v1/AncillaryOffers/AssistedTravel/r_", (string?)Row(answer, "assistedTravel", "A15")["optionsEndpoint"]);
    }

    [Fact]
    public async Task H04_FixedDirectHasSelectionIdAndRealPriceOnly()
    {
        var (_, rows) = await MixedAsync();

        var row = Row(Node(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api")), "baggages", "A05");

        Assert.StartsWith("d_", (string?)row["selectionId"]);
        Assert.Equal(40m, (decimal)row["unitPrice"]!["amount"]!);
        Assert.Null(row["reference"]);
        Assert.Null(row["quantityOptions"]);
        Assert.Null(row["optionsEndpoint"]);
    }

    [Fact]
    public async Task H05_QuantityOnlyBagHasQuantityOptionsAndNoSnapshot()
    {
        var (lab, rows) = await MixedAsync();

        var row = Row(Node(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api")), "baggages", "A01");

        Assert.Equal(2, row["quantityOptions"]!.AsArray().Count);
        Assert.Equal(200m, (decimal)row["quantityOptions"]![1]!["totalAmount"]!);
        Assert.Null(row["selectionId"]);
        Assert.Null(row["reference"]);
        Assert.Empty(lab.Shop.Catalog.Snapshots);
    }

    [Fact]
    public async Task H06_TypedWheelchairHasReferenceAndEndpointOnly()
    {
        var (_, rows) = await MixedAsync();

        var row = Row(Node(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api")), "assistedTravel", "A15");

        Assert.StartsWith("r_", (string?)row["reference"]);
        Assert.NotNull(row["optionsEndpoint"]);
        Assert.Null(row["selectionId"]);
        Assert.Null(row["quantityOptions"]);
    }

    [Fact]
    public async Task H07_QuoteUpgradeIsNotListed()
    {
        var lab = new V8Lab();
        var definition = lab.Shop.Definition("A10", confirmation: ConfirmationRequirement.Immediate);
        lab.Shop.Provision(definition);

        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
    }

    [Fact]
    public async Task H08_MedicalEquipmentIsNotListedEvenWithAFiledPrice()
    {
        var lab = new V8Lab();
        var definition = lab.Shop.Definition("A17");
        lab.Shop.Price(definition, lab.Shop.Provision(definition, disposition: CommercialDisposition.Paid), ShoppingLab.Rate(80m));

        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
    }

    [Fact]
    public async Task H09_ConvertedOrUnappliedOrWeightPricesAreNotListed()
    {
        var lab = new V8Lab();
        lab.Shop.Catalog.RatesOfExchange.Add(new PublishedRateOfExchange("ROE-1", ShoppingLab.Usd, ShoppingLab.Eur, 0.9m, 2, 1d));
        var converted = lab.Shop.Definition("A05", confirmation: ConfirmationRequirement.Immediate);
        lab.Shop.Price(converted, lab.Shop.Provision(converted), ShoppingLab.Rate(50m, ShoppingLab.Usd));
        var unapplied = lab.Shop.Definition("A01", confirmation: ConfirmationRequirement.Immediate);
        lab.Shop.Price(unapplied, lab.Shop.Provision(unapplied), ShoppingLab.Rate(50m, components: ShoppingLab.Fee("TKT", 5m, FeeApplicationUnit.Ticket)));
        var perKilogram = lab.Shop.Definition("A03", pricingUnit: PricingUnit.PerKilogram, confirmation: ConfirmationRequirement.Immediate);
        lab.Shop.Price(perKilogram, lab.Shop.Provision(perKilogram, quantityUnit: AncillaryQuantityUnit.Kilogram), ShoppingLab.Rate(5m));
        lab.Sellable("A21");

        var row = Assert.Single(await lab.ShopAsync(V8Lab.Context()));

        Assert.Equal("A21", row.Candidate.VariantCode);
    }

    [Fact]
    public async Task H10_ExtraSeatIsGatedAndItsArrayIsEmpty()
    {
        var lab = new V8Lab();
        lab.Sellable("A09");

        var rows = await lab.ShopAsync(V8Lab.Context());

        Assert.Empty(rows);
        Assert.Empty(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api").ExtraSeats);
    }

    [Fact]
    public async Task H11_TravelInsuranceArrayIsEmpty()
    {
        var (_, rows) = await MixedAsync();

        Assert.Empty(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api").TravelInsurance);
        Assert.Equal(24, AncillaryVariant.All.Count);
    }

    [Fact]
    public async Task H12_CurrencyIsTheInternalCatalogId()
    {
        var (_, rows) = await MixedAsync();

        var answer = Node(AncillaryOffersMapper.Map(V8Lab.Snapshot(rows), "Api"));

        Assert.Equal(ShoppingLab.Eur, (int)answer["currencyId"]!);
        Assert.Equal(47, (int)Row(answer, "baggages", "A05")["unitPrice"]!["currencyId"]!);
    }

    [Fact]
    public async Task H13_SuspendedOrRetiredPricingIsNeverListed()
    {
        var lab = new V8Lab();
        var (_, _, suspended) = lab.Sellable("A05");
        var (_, _, retired) = lab.Sellable("A21");

        suspended.Suspend(ShoppingLab.Now);
        retired.Retire(ShoppingLab.Now);

        Assert.Empty(await lab.ShopAsync(V8Lab.Context()));
    }
}
