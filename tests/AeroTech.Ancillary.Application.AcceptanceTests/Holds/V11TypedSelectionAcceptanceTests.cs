using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V11TypedSelectionAcceptanceTests
{
    private const long Flight = 301;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V11TypedSelectionAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 1_120_000;

    private async Task<IReadOnlyDictionary<string, long>> CatalogAsync(params string[] codes)
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var provisions = new Dictionary<string, long>(StringComparer.Ordinal);

        foreach (var code in codes)
            provisions[code] = await PublishedAsync(fixture, airlineId, supplierId, V122Catalog.Case(code));

        return provisions;
    }

    private async Task<long> PublishedAsync(InventoryFixture fixture, int airlineId, long supplierId, VariantCase variant)
    {
        var fixedUpgrade = variant.Code == AncillaryVariant.CabinUpgrade;
        var definition = await _harness.Proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, supplierId) with
        {
            Booking = variant.Booking with { ConfirmationRequirement = ConfirmationRequirement.Immediate },
            TypedSpecification = fixedUpgrade
                ? new ServiceSpecificationInput(Upgrade: new UpgradeSpecificationInput(1, 2, UpgradeKind.FixedAncillary, true, [5]))
                : variant.Specification
        });
        var rule = await _harness.Proof.RuleAsync(
            V122Catalog.Rule(variant, definition.Id) with { Origin = fixedUpgrade ? PriceOrigin.Filed : variant.PriceOrigin, QuoteProviderKey = null },
            variant.Disposition == CommercialDisposition.Paid
                ? provisionId => new TestDefinePricingRatesCommand(provisionId, [new(null, null, null, new MoneyInput(25m, Eur), null)])
                : null);

        await _harness.ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, variant.Reference, definition.Id, InventoryAuthority.Unlimited));

        return rule.Provision.Id;
    }

    private static HoldSelection Unit(string code, long provisionId, long orderServiceId, ISelectionOptions? options = null, int? quantity = null)
        => new(V122Catalog.ProfileOf(code), code, provisionId, orderServiceId, 71, [Flight], quantity, options);

    private TestHoldAncillaryServicesCommand Hold(long orderId, string key, params HoldSelection[] selections)
        => M1Commands.Hold($"V11-{orderId}-{key}", orderId, _clock.Now.AddMinutes(30), selections);

    [Fact]
    public async Task V11_D01_D10_D21_W17_W18_every_family_reads_back_its_chosen_and_fixed_values_from_sql()
    {
        var catalog = await CatalogAsync("A03", "A06", "A10", "A11", "A12", "A15", "A20", "A23", "A24");
        var orderId = NextOrderId;
        var service = orderId * 10;
        await using var scope = new AncillaryScope(_database, _clock);

        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(
            orderId,
            "ALL",
            Unit("A03", catalog["A03"], service + 1, new BaggageSelectionOptions("B-32", 27.5m, null, null), 1),
            Unit("A06", catalog["A06"], service + 2, new BaggageSelectionOptions(null, null, new SelectedDimensionsInput(150m, 60m, 30m), 18.25m)),
            Unit("A10", catalog["A10"], service + 3),
            Unit("A11", catalog["A11"], service + 4),
            Unit("A12", catalog["A12"], service + 5, quantity: 2),
            Unit("A15", catalog["A15"], service + 6, new AssistedTravelHoldSelectionOptions("WCHR")),
            Unit("A20", catalog["A20"], service + 7, new AirportServiceSelectionOptions(null, 1)),
            Unit("A23", catalog["A23"], service + 8, new PrioritySelectionOptions(true)),
            Unit("A24", catalog["A24"], service + 9, new ConnectivitySelectionOptions(2))));
        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        var baggages = read.Baggages!.ToDictionary(variant => variant.VariantCode);
        var overweight = Assert.Single(baggages["A03"].Products);
        var bike = Assert.Single(baggages["A06"].Products);
        var bikeOptions = Assert.Single(bike.Units).Selected.Options!.Baggage;
        var bikeSize = bikeOptions.Dimensions!;
        var overweightUnit = Assert.Single(overweight.Units);

        Assert.Equal(("OVERWEIGHT_32", null), (overweight.ServiceDefinitionRef, overweight.Details));
        Assert.Equal((1, "B-32", (decimal?)27.5m), (overweightUnit.Selected.Quantity, overweightUnit.Selected.Options!.Baggage.BagRef, overweightUnit.Selected.Options.Baggage.MeasuredWeightKg));
        Assert.Equal("BIKE", bike.Details!.EquipmentKind);
        Assert.Equal((150m, 60m, 30m, (decimal?)18.25m), (bikeSize.LengthCm, bikeSize.WidthCm, bikeSize.HeightCm, bikeOptions.WeightKg));

        var meals = read.Meals!.ToDictionary(variant => variant.VariantCode);
        var special = Assert.Single(meals["A11"].Products);
        var preorder = Assert.Single(meals["A12"].Products);

        Assert.Equal(("VGML", null), (special.Details!.MealCode, special.Details.MenuItemRef));
        Assert.Equal((null, "MENU_PASTA_01", 2), (preorder.Details!.MealCode, preorder.Details.MenuItemRef, Assert.Single(preorder.Units).Selected.Quantity));
        Assert.Equal("""{"quantity":2}""", JsonSerializer.Serialize(Assert.Single(preorder.Units).Selected, HostJson.Options));
        Assert.Equal(2, Assert.Single(Assert.Single(read.Upgrades!).Products).Details!.ToCabinId);

        var wheelchair = Assert.Single(Assert.Single(Assert.Single(read.AssistedTravel!).Products).Units);
        var lounge = Assert.Single(Assert.Single(read.AirportServices!).Products);

        Assert.Equal("WCHR", wheelchair.Selected.Options!.AssistedTravel.AssistanceSsrCode);
        Assert.Equal((Thr, (long?)null), (lounge.Details!.AirportId, lounge.Details.FacilityId));
        Assert.Equal(
            """{"quantity":1,"options":{"airportService":{"guestCount":1}}}""",
            JsonSerializer.Serialize(Assert.Single(lounge.Units).Selected, HostJson.Options));
        Assert.True(Assert.Single(Assert.Single(Assert.Single(read.PriorityServices!).Products).Units).Selected.Options!.Priority.OptIn);
        Assert.Equal(2, Assert.Single(Assert.Single(Assert.Single(read.Connectivity!).Products).Units).Selected.Options!.Connectivity.DeviceCount);
        Assert.Null(read.Seats);
        Assert.All(read.Units(), unit =>
        {
            Assert.Equal(71L, unit.Unit!.TravellerId);
            Assert.Equal(new long[] { Flight }, unit.Coverage!.FlightIds);
        });

        Assert.Equal(
            new[] { "1|1|0|1|1|1" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(
                    (SELECT COUNT(*) FROM Ancillary.AncillaryReservationUnitAssistedTravelSelections AS row JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.Id = row.AncillaryReservationUnitId WHERE unit.AncillaryReservationId = {hold.HoldId}), '|',
                    (SELECT COUNT(*) FROM Ancillary.AncillaryReservationUnitConnectivitySelections AS row JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.Id = row.AncillaryReservationUnitId WHERE unit.AncillaryReservationId = {hold.HoldId}), '|',
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Ancillary' AND TABLE_NAME LIKE 'AncillaryReservation%' AND CHARACTER_MAXIMUM_LENGTH = -1), '|',
                    (SELECT COUNT(*) FROM Ancillary.AncillaryReservationUnitUpgradeSelections AS row JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.Id = row.AncillaryReservationUnitId WHERE unit.AncillaryReservationId = {hold.HoldId}), '|',
                    (SELECT COUNT(*) FROM Ancillary.AncillaryReservationUnitAirportServiceSelections AS row JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.Id = row.AncillaryReservationUnitId WHERE unit.AncillaryReservationId = {hold.HoldId}), '|',
                    (SELECT COUNT(*) FROM Ancillary.AncillaryReservationUnitFlights AS row JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.Id = row.AncillaryReservationUnitId WHERE unit.AncillaryReservationId = {hold.HoldId} AND unit.OrderServiceId = {service + 1})) AS Value
                """));

        var wheelchairUnitId = hold.Units.Single(unit => unit.OrderServiceId == service + 6).UnitId;
        string before;

        await using (var first = new AncillaryScope(_database, _clock))
            before = JsonSerializer.Serialize(await first.GetHoldById.ExecuteAsync(hold.HoldId), HostJson.Options);

        await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId, [wheelchairUnitId]));

        foreach (var provisionId in catalog.Values)
            await scope.RetireProvision.RetireAsync(new TestProvisionLifecycleCommand(provisionId));

        await using var reader = new AncillaryScope(_database, _clock);
        var after = await reader.GetHoldById.ExecuteAsync(hold.HoldId);
        var released = Assert.Single(Assert.Single(Assert.Single(after.AssistedTravel!).Products).Units);

        Assert.Equal((AncillaryReservationUnitStatus.Released, "WCHR"), (released.Status, released.Selected.Options!.AssistedTravel.AssistanceSsrCode));
        Assert.Equal(
            before,
            JsonSerializer.Serialize(after, HostJson.Options).Replace($"\"unitId\":\"{wheelchairUnitId}\",\"orderServiceId\":\"{service + 6}\",\"status\":\"Released\"", $"\"unitId\":\"{wheelchairUnitId}\",\"orderServiceId\":\"{service + 6}\",\"status\":\"Held\""));
    }

    [Fact]
    public async Task V11_D23_an_option_outside_the_definition_rules_is_refused_before_any_write()
    {
        var catalog = await CatalogAsync("A03", "A06", "A15", "A23", "A24");
        var orderId = NextOrderId;
        var service = orderId * 10 + 1;
        await using var scope = new AncillaryScope(_database, _clock);

        Task Refused(string key, HoldSelection selection) => BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldAncillaryServices.HoldAsync(Hold(orderId, key, selection)));

        await Refused("SSR", Unit("A15", catalog["A15"], service, new AssistedTravelHoldSelectionOptions("XXXX")));
        await Refused("SSR-MISSING", Unit("A15", catalog["A15"], service));
        await Refused("DEVICES", Unit("A24", catalog["A24"], service, new ConnectivitySelectionOptions(3)));
        await Refused("WEIGHT", Unit("A03", catalog["A03"], service, new BaggageSelectionOptions("B-1", 23m, null, null), 1));
        await Refused("BAGREF", Unit("A03", catalog["A03"], service, new BaggageSelectionOptions(null, 27m, null, null), 1));
        await Refused("FIELD", Unit("A06", catalog["A06"], service, new BaggageSelectionOptions("B-1", null, new SelectedDimensionsInput(100m, 50m, 30m), 10m)));
        await Refused("OPT-IN", Unit("A23", catalog["A23"], service, new PrioritySelectionOptions(false)));

        Assert.Equal(
            new[] { "0" },
            await _harness.RowsAsync($"SELECT CONCAT(COUNT(*), '') AS Value FROM Ancillary.AncillaryReservations WHERE OrderId = {orderId}"));
    }

    [Fact]
    public void V11_every_application_validator_can_be_built_by_the_container()
    {
        using var provider = new ServiceCollection()
            .AddValidatorsFromAssembly(typeof(HoldUnitInputValidator).Assembly)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<ServiceHoldAncillaryServicesCommand>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<SelectedUnitInput>>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<BaggageHoldOptionsInput>>());
    }
}
