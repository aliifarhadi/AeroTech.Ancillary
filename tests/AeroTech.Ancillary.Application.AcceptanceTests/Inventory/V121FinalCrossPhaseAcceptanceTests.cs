using System.Security.Cryptography;
using System.Text;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Entities;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

[Collection(DatabaseCollection.Name)]
public class V121FinalCrossPhaseAcceptanceTests
{
    private const long Flight = 81234;
    private const string NoSources = "0|0|0|0|0|0";
    private const string FrozenSourceHash = "BB7E5038C5CDE87014A85FEFC94801D779241782DD242F31875E4AD4EA9890B7";

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V121FinalCrossPhaseAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private Task<InventoryPolicyResult> DefineAsync(InventoryFixture fixture, TestDefineInventoryPolicyCommand command)
        => _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command));

    private Task<InventoryPolicyResult> ActivateAsync(InventoryFixture fixture, long policyId, long expectedVersion = 1)
        => _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policyId, expectedVersion)));

    private async Task<long> ActivePolicyAsync(InventoryFixture fixture, TestDefineInventoryPolicyCommand command)
        => (await ActivateAsync(fixture, (await DefineAsync(fixture, command)).Id)).Id;

    private Task<FlightCountInventoryResult> CountAsync(InventoryFixture fixture, int airlineId, long flightId, int total, long resourceId = PetResource)
        => _harness.RequestAsync(fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, flightId, resourceId, InventoryCountUnit.AnimalCarrier, total)));

    private Task<FlightWeightInventoryResult> WeightAsync(InventoryFixture fixture, int airlineId, long flightId, decimal capacityKg)
        => _harness.RequestAsync(fixture, scope => scope.DefineFlightWeightInventory.DefineAsync(
            new TestDefineFlightWeightInventoryCommand(airlineId, flightId, BagWeightResource, capacityKg)));

    private Task<AirportSlotInventoryResult> SlotAsync(InventoryFixture fixture, int airlineId, DateTimeOffset startUtc, DateTimeOffset endUtc, int capacity = 12, long facilityId = Lounge)
        => _harness.RequestAsync(fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, facilityId, startUtc, endUtc, capacity)));

    private async Task<(InventoryFixture Fixture, int AirlineId, long SupplierId)> VerifiedAsync()
    {
        var connected = await _harness.OperatorAsync();

        connected.Fixture.Flights!.Add(Flight);
        connected.Fixture.Resources!.UnionWith([(InventoryResourceKind.FlightCount, PetResource), (InventoryResourceKind.FlightWeight, BagWeightResource)]);
        connected.Fixture.Airports.Add(Ika);
        connected.Fixture.Facilities![Lounge] = (Ika, "Asia/Tehran");
        connected.Fixture.CountingFamilies!.Add("MEAL");

        return connected;
    }

    private async Task<string> SourceRowsAsync(int airlineId)
        => (await _harness.RowsAsync($"""
            SELECT CONCAT(
                (SELECT COUNT(*) FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.AirportSlotInventories WHERE OwnerAirlineId = {airlineId})) AS Value
            """)).Single();

    private async Task<string> CommercialRowsAsync(int airlineId)
        => string.Join(
            '\n',
            await _harness.RowsAsync($"""
                SELECT CONCAT('definition ', definition.Id, ' ', definition.Version, ' ', definition.Status, ' ', CONVERT(varchar(33), definition.LastUpdateTime, 126)) AS Value
                FROM Ancillary.AncillaryServiceDefinitions AS definition WHERE definition.OwnerAirlineId = {airlineId}
                UNION ALL
                SELECT CONCAT('provision ', provision.Id, ' ', provision.Sequence, ' ', provision.Status, ' ', provision.PurchaseStage, ' ', CONVERT(varchar(33), provision.LastUpdateTime, 126))
                FROM Ancillary.AncillaryProvisions AS provision
                JOIN Ancillary.AncillaryServiceDefinitions AS definition ON definition.Id = provision.ServiceDefinitionId WHERE definition.OwnerAirlineId = {airlineId}
                UNION ALL
                SELECT CONCAT('pricing ', pricing.Id, ' ', pricing.Version, ' ', pricing.Status, ' ', rate.Id, ' ', rate.CurrencyId, ' ', rate.BaseAmount, ' ', CONVERT(varchar(33), pricing.LastUpdateTime, 126))
                FROM Ancillary.AncillaryPricings AS pricing
                JOIN Ancillary.AncillaryPricingRates AS rate ON rate.AncillaryPricingId = pricing.Id
                JOIN Ancillary.AncillaryProvisions AS provision ON provision.Id = pricing.AncillaryProvisionId
                JOIN Ancillary.AncillaryServiceDefinitions AS definition ON definition.Id = provision.ServiceDefinitionId WHERE definition.OwnerAirlineId = {airlineId}
                ORDER BY 1
                """));

    [Fact]
    public async Task X02_not_configured_is_never_reported_as_unlimited()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync(connected: false);
        var meal = await _harness.ProductAsync(airlineId, supplierId, "MEAL_VGML");
        var missing = await _harness.SnapshotAsync(fixture, "MEAL_VGML", Flight);
        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "MEAL_VGML", meal.Id, InventoryAuthority.Unlimited));
        var authored = await _harness.SnapshotAsync(fixture, "MEAL_VGML", Flight);

        Assert.Equal(("NotConfigured", "PolicyNotConfigured", false), (missing.State.Name, missing.ReasonCode, missing.IsGuaranteed));
        Assert.Null(missing.Authority);
        Assert.Equal(("NotConfigured", "PolicyNotActive", draft.Id), (authored.State.Name, authored.ReasonCode, authored.PolicyId!.Value));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "NEVER_DEFINED")).State.Name);
    }

    [Fact]
    public async Task X04_a_seat_product_never_owns_an_inventory_policy_or_a_local_occupancy_source()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var seat = await _harness.Proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, "SEAT_SELECTION", "SXL", "F", "SA", "SEAT_SELECTION", pricingUnit: PricingUnit.PerSeat, variant: "A07"));

        await _harness.RefusedAsync(16620, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, null, Count())));
        await _harness.RefusedAsync(16620, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.Unlimited)));

        var snapshot = await _harness.SnapshotAsync(fixture, "SEAT_SELECTION", Flight);

        Assert.Equal(("NotApplicable", "SeatCapacityOwnedExternally", false, seat.Id), (snapshot.State.Name, snapshot.ReasonCode, snapshot.IsGuaranteed, snapshot.CurrentServiceDefinitionId));
        Assert.Null(snapshot.Authority);
        Assert.Null(snapshot.PolicyId);
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.Resource);
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
        Assert.Empty(await _harness.RowsAsync($"SELECT t.name AS Value FROM sys.tables AS t WHERE t.name LIKE '%Seat%' AND t.name NOT LIKE '%Provision%' AND t.name NOT LIKE '%Specification%' AND t.name NOT LIKE 'AssistedTravel%'"));
    }

    [Fact]
    public async Task X05_an_unknown_facility_keeps_the_airport_slot_a_draft_and_refuses_its_activation_explicitly()
    {
        var (fixture, airlineId, _) = await _harness.OperatorAsync();

        fixture.Airports.Add(Ika);

        var slot = await SlotAsync(fixture, airlineId, Utc(10), Utc(11));
        var offline = new InventoryFixture(airlineId);

        offline.Airports.Add(Ika);
        Assert.Equal((InventoryRecordStatus.Draft, 1L), (slot.Status, slot.Version));
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(slot.Id, 1)));
        await _harness.RefusedAsync(16608, 409, offline, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(slot.Id, 1)));
        Assert.Equal(
            new[] { "1|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Status, '|', Version) AS Value FROM Ancillary.AirportSlotInventories WHERE Id = {slot.Id}"));
    }

    [Fact]
    public async Task X06_a_missing_source_of_truth_refuses_the_activation_of_a_flight_count_and_a_flight_weight_source()
    {
        var (_, airlineId, _) = await _harness.OperatorAsync();
        var offline = new InventoryFixture(airlineId);
        var count = await CountAsync(offline, airlineId, Flight, 2);
        var weight = await WeightAsync(offline, airlineId, Flight, 500m);

        await _harness.RefusedAsync(16608, 409, offline, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(count.Id, 1)));
        await _harness.RefusedAsync(16608, 409, offline, scope => scope.ActivateFlightWeightInventory.ActivateAsync(new TestFlightWeightLifecycleCommand(weight.Id, 1)));

        var unknown = InventoryFixture.Connected(airlineId);

        await _harness.RefusedAsync(16609, 422, unknown, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(count.Id, 1)));
        Assert.Equal(
            new[] { "count 1|1", "weight 1|1" },
            await _harness.RowsAsync($"""
                SELECT CONCAT('count ', Status, '|', Version) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {count.Id}
                UNION ALL SELECT CONCAT('weight ', Status, '|', Version) FROM Ancillary.FlightWeightInventories WHERE Id = {weight.Id}
                """));
    }

    [Fact]
    public async Task X07_X08_an_adjustment_replays_idempotently_for_the_same_correlation_and_conflicts_when_the_payload_differs()
    {
        var (fixture, airlineId, _) = await VerifiedAsync();
        var source = await CountAsync(fixture, airlineId, Flight, 8);
        var adjusted = await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "x07", 1)));
        var replayed = await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "x07", 1)));

        Assert.Equal(adjusted, replayed);
        await _harness.RefusedAsync(16616, 409, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 6, "AIRCRAFT_CHANGE", "x07", 1)));
        await _harness.RefusedAsync(16616, 409, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "x07", 2)));
        Assert.Equal(
            new[] { "5|2|1" },
            await _harness.RowsAsync($"SELECT CONCAT(TotalCapacity, '|', Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments WHERE FlightCountInventoryId = {source.Id})) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {source.Id}"));
    }

    [Fact]
    public async Task X09_one_hundred_writes_with_the_same_expected_version_have_one_winner()
    {
        var (fixture, airlineId, _) = await VerifiedAsync();
        var source = await CountAsync(fixture, airlineId, Flight, 8);
        var scopes = Enumerable.Range(0, 100).Select(_ => new InventoryScope(_database, _clock, fixture)).ToList();

        foreach (var scope in scopes)
            await scope.FlightCountInventories.GetAsync(source.Id);

        var raced = await _harness.RaceAsync(scopes.Select((scope, index) => (Func<Task>)(() => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 100 + index, "RACE", $"x09-{index}", 1)))));

        foreach (var scope in scopes)
            await scope.DisposeAsync();

        Assert.Equal(1, raced.Succeeded);
        Assert.Equal(new[] { 16605 }, raced.Codes);
        Assert.Equal(
            new[] { "2|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments WHERE FlightCountInventoryId = {source.Id})) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {source.Id}"));
    }

    [Fact]
    public async Task X12_one_hundred_independent_sources_are_all_accepted_concurrently()
    {
        var (fixture, airlineId, _) = await VerifiedAsync();
        var raced = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(index => (Func<Task>)(() => CountAsync(fixture, airlineId, 70000 + index, 4))));

        Assert.Equal(100, raced.Succeeded);
        Assert.Empty(raced.Codes);
        Assert.Equal("100", (await _harness.RowsAsync($"SELECT CAST(COUNT(DISTINCT FlightId) AS varchar(10)) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}")).Single());
    }

    [Fact]
    public async Task X15_a_count_plus_weight_policy_binds_exactly_one_count_and_one_weight_resource()
    {
        var (fixture, airlineId, supplierId) = await VerifiedAsync();
        var bag = await _harness.ProductAsync(airlineId, supplierId, "XBAG_OVERSIZE", PricingUnit.PerItem);

        Assert.Equal(
            new[] { typeof(FlightCountConsumptionInput), typeof(FlightWeightConsumptionInput) },
            new[] { nameof(IDefineInventoryPolicyCommand.CountConsumption), nameof(IDefineInventoryPolicyCommand.WeightConsumption) }
                .Select(name => typeof(IDefineInventoryPolicyCommand).GetProperty(name)!.PropertyType));

        var countOnly = await DefineAsync(
            fixture,
            new TestDefineInventoryPolicyCommand(airlineId, "XBAG_OVERSIZE", bag.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCountPlusWeight, CountConsumption: Count()));

        await _harness.RefusedAsync(16606, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(countOnly.Id, 1)));

        var both = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(new TestChangeInventoryPolicyCommand(
            countOnly.Id,
            bag.Id,
            InventoryAuthority.Local,
            1,
            LocalInventoryPattern.FlightCountPlusWeight,
            CountConsumption: Count(),
            WeightConsumption: FixedKg(5m))));
        var active = await ActivateAsync(fixture, both.Id, both.Version);
        var detail = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(active.Id));

        Assert.Equal(InventoryRecordStatus.Active, active.Status);
        Assert.Equal((PetResource, BagWeightResource, (decimal?)5m), (detail.CountConsumption!.ResourceId, detail.WeightConsumption!.WeightResourceId, detail.WeightConsumption.FixedKgPerUnit));
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task X16_a_passenger_usage_limit_is_an_entitlement_limit_and_never_a_physical_total_or_a_usage_ledger()
    {
        var (fixture, airlineId, supplierId) = await VerifiedAsync();
        var meal = await _harness.ProductAsync(airlineId, supplierId, "MEAL_PREORDER_HOT");
        var policyId = await ActivePolicyAsync(
            fixture,
            new TestDefineInventoryPolicyCommand(
                airlineId,
                "MEAL_PREORDER_HOT",
                meal.Id,
                InventoryAuthority.Unlimited,
                PassengerUsageLimits: [Limit(PassengerUsageLimitScope.PerFlightOccurrence, 1, "MEAL")]));
        var detail = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(policyId));
        var snapshot = await _harness.SnapshotAsync(fixture, "MEAL_PREORDER_HOT", Flight);

        Assert.Equal(("PerFlightOccurrence", 1, "MEAL"), detail.PassengerUsageLimits.Select(limit => (limit.LimitScope.Name, limit.MaxUnits, limit.CountingFamilyCode)).Single());
        Assert.Equal(("Unlimited", (int?)null, false), (snapshot.State.Name, snapshot.ConfiguredCount, snapshot.IsGuaranteed));
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
        Assert.DoesNotContain(
            typeof(PassengerUsageLimit).GetProperties(),
            property => new[] { "Used", "Consumed", "Remaining", "Available", "Total" }.Any(term => property.Name.Contains(term, StringComparison.Ordinal)));
        Assert.Empty(await _harness.RowsAsync($"SELECT t.name AS Value FROM sys.tables AS t WHERE t.name LIKE '%Usage%' AND t.name NOT LIKE '%PassengerUsageLimits'"));
    }

    [Fact]
    public void X20_the_reservation_source_and_its_hold_get_and_confirm_use_cases_are_byte_stable()
    {
        static bool InFolder(string path, string folder)
            => path.Contains($"{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

        static string[] Folders(params string[] segments) => Directory
            .EnumerateDirectories(Path.Combine([RepositoryFiles.Root, "src", .. segments]))
            .Select(directory => Path.GetFileName(directory)!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var frozen = new[] { "src", "tests" }
            .SelectMany(root => Directory.EnumerateFiles(Path.Combine(RepositoryFiles.Root, root), "*.cs", SearchOption.AllDirectories))
            .Where(path => !InFolder(path, "obj") && !InFolder(path, "bin"))
            .Where(path => InFolder(path, "AncillaryReservationAggregate")
                           || Path.GetFileName(path) is "M1ReservationAcceptanceTests.cs" or "M1ReservationConformanceTests.cs")
            .Select(path => (
                Path: Path.GetRelativePath(RepositoryFiles.Root, path).Replace('\\', '/'),
                Hash: Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path).Where(value => value != 13).ToArray()))))
            .OrderBy(file => file.Path, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(133, frozen.Count);
        Assert.Equal(
            FrozenSourceHash,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', frozen.Select(file => $"{file.Path}:{file.Hash}"))))));
        Assert.Equal(
            new[] { "CancelAncillaryHold", "ConfirmAncillaryHold", "ExpireAncillaryHolds", "HoldAncillaryServices", "HoldSeatSelections", "ReleaseAncillaryHold" },
            Folders("AeroTech.Ancillary.Application", "AncillaryReservationAggregate", "Commands"));
        Assert.Equal(new[] { "GetAncillaryHoldById" }, Folders("AeroTech.Ancillary.Query", "AncillaryReservationAggregate", "Queries"));
    }
}
