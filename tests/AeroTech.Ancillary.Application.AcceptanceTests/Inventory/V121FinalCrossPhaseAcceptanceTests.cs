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
    private const string FrozenSourceHash = "6974ABA2C62BAFE7CDE159BDCE8A4969F1CAECCF25368012F30D1BE7DDA4E486";

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
    public async Task X01_unlimited_and_must_check_availability_coexist_without_a_local_count_and_without_a_guarantee()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync(connected: false);
        var wheelchair = await _harness.ProductAsync(airlineId, supplierId, "ASSIST_WCHR");

        await _harness.Proof.RuleAsync(Provision(wheelchair.Id, 10, CommercialDisposition.Free) with { Availability = new(true) });

        var active = await ActivateAsync(fixture, (await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "ASSIST_WCHR", wheelchair.Id, InventoryAuthority.Unlimited))).Id);
        var snapshot = await _harness.SnapshotAsync(fixture, "ASSIST_WCHR", Flight);

        Assert.Equal((InventoryRecordStatus.Active, InventoryAuthority.Unlimited), (active.Status, active.Authority));
        Assert.Equal(("Unlimited", true, false), (snapshot.State.Name, snapshot.RequiresAvailabilityCheck, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.ConfiguredKg);
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
    }

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
    public async Task X03_a_baggage_sale_type_never_seeds_a_flight_count_or_a_flight_weight_source()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync(connected: false);
        var package = await _harness.ProductAsync(airlineId, supplierId, "XBAG_WEIGHT_10KG", PricingUnit.PerItem);
        var piece = await _harness.ProductAsync(airlineId, supplierId, "XBAG_PIECE_23KG", PricingUnit.PerPiece);

        await _harness.Proof.RuleAsync(
            Provision(package.Id, 10, maxQuantity: 2, applicationType: ProvisionApplicationType.Baggage) with
            {
                BaggageApplication = Baggage(10m, chargeKind: BaggageChargeKind.WeightPackage, allowanceConcept: BaggageAllowanceConcept.Weight)
            },
            id => Pricing(id, Eur, Base(25m)));
        await _harness.Proof.RuleAsync(
            Provision(piece.Id, 10, quantityUnit: AncillaryQuantityUnit.Piece, maxQuantity: 3, applicationType: ProvisionApplicationType.Baggage) with { BaggageApplication = Baggage(23m, 1, 3) },
            id => Pricing(id, Eur, Base(30m)));

        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "XBAG_WEIGHT_10KG", Flight)).State.Name);

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "XBAG_WEIGHT_10KG", package.Id, InventoryAuthority.Unlimited));
        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "XBAG_PIECE_23KG", piece.Id, InventoryAuthority.Unlimited));

        var declared = await _harness.SnapshotAsync(fixture, "XBAG_WEIGHT_10KG", Flight);

        Assert.Equal(("Unlimited", false), (declared.State.Name, declared.IsGuaranteed));
        Assert.Null(declared.ConfiguredKg);
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task X04_a_seat_product_never_owns_a_local_occupancy_source()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var seat = await _harness.ProductAsync(airlineId, supplierId, "SEAT_SELECTION", PricingUnit.PerSeat);

        fixture.FlightFlowProviderKeys!.Add("FlightFlow");
        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.FlightFlow, LocalInventoryPattern.FlightCount, "FlightFlow", Count())));

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.FlightFlow, ProviderKey: "FlightFlow"));

        var snapshot = await _harness.SnapshotAsync(fixture, "SEAT_SELECTION", Flight);

        Assert.Equal(("DelegatedCheckRequired", "FlightFlow", false), (snapshot.State.Name, snapshot.Authority!.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.Resource);
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
        Assert.Empty(await _harness.RowsAsync($"SELECT t.name AS Value FROM sys.tables AS t WHERE t.name LIKE '%Seat%' AND t.name NOT LIKE '%Provision%'"));
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
    public async Task X10_X11_one_hundred_overlapping_slots_of_one_facility_have_one_winner_and_adjacent_slots_are_accepted()
    {
        var (fixture, airlineId, _) = await VerifiedAsync();
        var overlapping = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(index => (Func<Task>)(() => SlotAsync(
            fixture,
            airlineId,
            Utc(8).AddMinutes(index),
            Utc(11).AddMinutes(index)))));

        Assert.Equal(1, overlapping.Succeeded);
        Assert.Equal(new[] { 16615 }, overlapping.Codes);

        fixture.Facilities![SecondLounge] = (Ika, "Asia/Tehran");

        var first = await SlotAsync(fixture, airlineId, Utc(10), Utc(11), facilityId: SecondLounge);
        var adjacent = await SlotAsync(fixture, airlineId, Utc(11), Utc(12), facilityId: SecondLounge);

        await _harness.RefusedAsync(16615, 409, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, SecondLounge, Utc(10, 30), Utc(11, 30), 12)));
        Assert.Equal((first.EndUtc, InventoryRecordStatus.Draft), (adjacent.StartUtc, adjacent.Status));
        Assert.Equal(
            new[] { "3|0" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(
                    (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId} AND Status <> 4), '|',
                    (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories AS first
                     JOIN Ancillary.AirportSlotInventories AS second
                       ON second.OwnerAirlineId = first.OwnerAirlineId AND second.FacilityId = first.FacilityId AND second.Id > first.Id
                      AND second.Status <> 4 AND first.StartUtc < second.EndUtc AND second.StartUtc < first.EndUtc
                     WHERE first.OwnerAirlineId = {airlineId} AND first.Status <> 4)) AS Value
                """));
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
    public async Task X13_a_new_product_version_keeps_the_one_policy_and_its_physical_source_and_the_pointer_is_reconciled_at_activation()
    {
        var (fixture, airlineId, supplierId) = await VerifiedAsync();
        var first = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);
        var policyId = await ActivePolicyAsync(
            fixture,
            new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", first.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));
        var source = await CountAsync(fixture, airlineId, Flight, 2);

        await _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(source.Id, 1)));

        long secondId;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            secondId = (await author.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id))).Id;
            await author.RetireServiceDefinition.RetireAsync(new TestServiceDefinitionLifecycleCommand(first.Id));
            await author.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(secondId));
        }

        var kept = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyByServiceIdentity.ExecuteAsync("PET_IN_CABIN"));
        var snapshot = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight);

        Assert.NotEqual(first.Id, secondId);
        Assert.Equal((policyId, first.Id, "Active"), (kept.Id, kept.ServiceDefinitionId, kept.Status.Name));
        Assert.Equal((policyId, "ConfiguredNotGuaranteed", 2), (snapshot.PolicyId!.Value, snapshot.State.Name, snapshot.ConfiguredCount!.Value));
        await _harness.RefusedAsync(16604, 409, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", secondId, InventoryAuthority.Unlimited)));

        var suspended = await _harness.RequestAsync(fixture, scope => scope.SuspendInventoryPolicy.SuspendAsync(new TestInventoryPolicyLifecycleCommand(policyId, kept.Version)));
        var reconciled = await ActivateAsync(fixture, policyId, suspended.Version);

        Assert.Equal((policyId, secondId, InventoryRecordStatus.Active), (reconciled.Id, reconciled.ServiceDefinitionId, reconciled.Status));
        Assert.Equal(
            new[] { $"{policyId}|{secondId}|2|{source.Id}|2" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(policy.Id, '|', policy.ServiceDefinitionId, '|', policy.Status, '|', source.Id, '|', source.TotalCapacity) AS Value
                FROM ReadModel.AncillaryInventoryPolicies AS policy
                JOIN Ancillary.FlightCountInventories AS source ON source.OwnerAirlineId = policy.OwnerAirlineId AND source.ResourceId = policy.CountResourceId
                WHERE policy.OwnerAirlineId = {airlineId}
                """));
        Assert.Equal(secondId, (await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(policyId))).ServiceDefinitionId);
    }

    [Fact]
    public async Task X14_two_products_share_a_real_resource_only_through_their_explicit_typed_binding()
    {
        var (fixture, airlineId, supplierId) = await VerifiedAsync();
        var cabin = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);
        var small = await _harness.ProductAsync(airlineId, supplierId, "PET_SMALL_DOG", PricingUnit.PerItem);
        var unbound = await _harness.ProductAsync(airlineId, supplierId, "PET_TREATS", PricingUnit.PerItem);

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", cabin.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));
        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PET_SMALL_DOG", small.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));
        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PET_TREATS", unbound.Id, InventoryAuthority.Unlimited));

        var source = await CountAsync(fixture, airlineId, Flight, 2);

        await _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(source.Id, 1)));

        var first = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight);
        var second = await _harness.SnapshotAsync(fixture, "PET_SMALL_DOG", Flight);
        var third = await _harness.SnapshotAsync(fixture, "PET_TREATS", Flight);

        Assert.Equal((2, 2), (first.ConfiguredCount!.Value, second.ConfiguredCount!.Value));
        Assert.Equal(first.Resource, second.Resource);
        Assert.Equal(("Unlimited", (int?)null), (third.State.Name, third.ConfiguredCount));
        Assert.Null(third.Resource);
        Assert.Equal("1|0|0|1|0|0", await SourceRowsAsync(airlineId));
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

    [Theory]
    [InlineData(LocalInventoryPattern.DailyCount)]
    [InlineData(LocalInventoryPattern.RoomNight)]
    [InlineData(LocalInventoryPattern.AssignedAsset)]
    public async Task X17_a_deferred_daily_room_night_or_assigned_asset_pattern_is_never_activated(LocalInventoryPattern pattern)
    {
        var (fixture, airlineId, supplierId) = await VerifiedAsync();
        var reference = $"TOUR_{pattern}".ToUpperInvariant();
        var product = await _harness.ProductAsync(airlineId, supplierId, reference, PricingUnit.PerItem, ServiceDateBasis.ServiceStart);
        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, reference, product.Id, InventoryAuthority.Local, pattern));

        await _harness.RefusedAsync(16607, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1)));

        var snapshot = await _harness.SnapshotAsync(fixture, reference, Flight);

        Assert.Equal(("UnsupportedPattern", "PatternNotSupported", false), (snapshot.State.Name, snapshot.ReasonCode, snapshot.IsGuaranteed));
        Assert.Equal(NoSources, await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task X18_an_unavailable_source_is_never_reported_as_unlimited_or_available()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync(connected: false);
        var pet = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);
        var seat = await _harness.ProductAsync(airlineId, supplierId, "SEAT_SELECTION", PricingUnit.PerSeat);
        var local = await DefineAsync(
            fixture,
            new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", pet.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));
        var delegated = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.FlightFlow, ProviderKey: "FlightFlow"));

        await _harness.RefusedAsync(16608, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(local.Id, 1)));
        await _harness.RefusedAsync(16608, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(delegated.Id, 1)));

        var petSnapshot = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight);
        var seatSnapshot = await _harness.SnapshotAsync(fixture, "SEAT_SELECTION", Flight);

        Assert.Equal(
            (("NotConfigured", false, (int?)null), ("NotConfigured", false, (int?)null)),
            ((petSnapshot.State.Name, petSnapshot.IsGuaranteed, petSnapshot.ConfiguredCount), (seatSnapshot.State.Name, seatSnapshot.IsGuaranteed, seatSnapshot.ConfiguredCount)));
        Assert.DoesNotContain(Enum.GetNames<InventoryCapacityReadState>(), name => name.Contains("Available", StringComparison.Ordinal));
        Assert.Equal(
            new[] { "1", "1" },
            await _harness.RowsAsync($"SELECT CAST(Status AS varchar(10)) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId} ORDER BY Id"));
    }

    [Fact]
    public async Task X19_capacity_changes_leave_the_published_rules_and_prices_untouched()
    {
        var (fixture, airlineId, supplierId) = await VerifiedAsync();
        var pet = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);

        await _harness.Proof.RuleAsync(Provision(pet.Id, 10, bookingRequired: false) with { Availability = new(true) }, id => Pricing(id, Eur, Base(55m), Tax("VAT", 5.5m)));

        var before = await CommercialRowsAsync(airlineId);

        _clock.Now = _clock.Now.AddHours(2);

        var policyId = await ActivePolicyAsync(
            fixture,
            new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", pet.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));
        var source = await CountAsync(fixture, airlineId, Flight, 2);

        await _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(source.Id, 1)));
        await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(new TestAdjustFlightCountInventoryCommand(source.Id, 0, "CABIN_FULL", "x19", 2)));
        await _harness.RequestAsync(fixture, scope => scope.CloseFlightCountInventoryForSale.CloseAsync(new TestFlightCountLifecycleCommand(source.Id, 3)));
        await _harness.RequestAsync(fixture, scope => scope.SuspendInventoryPolicy.SuspendAsync(new TestInventoryPolicyLifecycleCommand(policyId, 2)));

        Assert.Equal(before, await CommercialRowsAsync(airlineId));
        Assert.Equal(3, before.Split('\n').Length);
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

        Assert.Equal(36, frozen.Count);
        Assert.Equal(
            FrozenSourceHash,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join('\n', frozen.Select(file => $"{file.Path}:{file.Hash}"))))));
        Assert.Equal(new[] { "ConfirmAncillaryHold", "HoldAncillaryServices" }, Folders("AeroTech.Ancillary.Application", "AncillaryReservationAggregate", "Commands"));
        Assert.Equal(new[] { "GetAncillaryHoldById" }, Folders("AeroTech.Ancillary.Query", "AncillaryReservationAggregate", "Queries"));
    }
}
