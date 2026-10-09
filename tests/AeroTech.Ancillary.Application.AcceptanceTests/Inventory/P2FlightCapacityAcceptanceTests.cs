using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Projection;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using AeroTech.Ancillary.Application._Shared.Authorization;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated.Backoffice;
using AeroTech.Ancillary.Synchronizer.FlightCountInventoryAggregate;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

[Collection(DatabaseCollection.Name)]
public class P2FlightCapacityAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public P2FlightCapacityAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private const long Flight = 81234;
    private const long OtherFlight = 81240;

    private Task<FlightCountInventoryResult> CountAsync(InventoryFixture fixture, int airlineId, long flightId, long resourceId, InventoryCountUnit unit, int total)
        => _harness.RequestAsync(fixture, scope => scope.DefineFlightCountInventory.DefineAsync(new TestDefineFlightCountInventoryCommand(airlineId, flightId, resourceId, unit, total)));

    private Task<FlightWeightInventoryResult> WeightAsync(InventoryFixture fixture, int airlineId, long flightId, decimal capacityKg, long resourceId = BagWeightResource)
        => _harness.RequestAsync(fixture, scope => scope.DefineFlightWeightInventory.DefineAsync(new TestDefineFlightWeightInventoryCommand(airlineId, flightId, resourceId, capacityKg)));

    private Task<FlightCountInventoryResult> ActivateCountAsync(InventoryFixture fixture, long inventoryId, long expectedVersion)
        => _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(inventoryId, expectedVersion)));

    private Task<FlightWeightInventoryResult> ActivateWeightAsync(InventoryFixture fixture, long inventoryId, long expectedVersion)
        => _harness.RequestAsync(fixture, scope => scope.ActivateFlightWeightInventory.ActivateAsync(new TestFlightWeightLifecycleCommand(inventoryId, expectedVersion)));

    private async Task<long> ActivePolicyAsync(InventoryFixture fixture, TestDefineInventoryPolicyCommand command)
    {
        var draft = await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command));

        return (await _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1)))).Id;
    }

    private async Task<(InventoryFixture Fixture, int AirlineId, long SupplierId)> ConnectedAsync()
    {
        var connected = await _harness.OperatorAsync();

        connected.Fixture.Flights!.UnionWith([Flight, OtherFlight]);
        connected.Fixture.Resources!.UnionWith(
        [
            (InventoryResourceKind.FlightCount, PetResource),
            (InventoryResourceKind.FlightCount, MealResource),
            (InventoryResourceKind.FlightCount, OversizeResource),
            (InventoryResourceKind.FlightWeight, BagWeightResource)
        ]);

        return connected;
    }

    [Fact]
    public async Task P2_F01_F03_F05_a_flight_count_source_is_one_row_per_owner_flight_and_resource_enforced_by_the_database()
    {
        var (fixture, airlineId, _) = await ConnectedAsync();
        var first = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 4);
        var second = await CountAsync(fixture, airlineId, OtherFlight, PetResource, InventoryCountUnit.AnimalCarrier, 2);

        Assert.Equal((InventoryRecordStatus.Draft, 1L, 4, Flight, PetResource), (first.Status, first.Version, first.TotalCapacity, first.FlightId, first.ResourceId));
        Assert.NotEqual(first.Id, second.Id);
        await _harness.RefusedAsync(16614, 409, fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 9)));
        Assert.Equal(
            new[] { $"{Flight}|{PetResource}|4|4|1", $"{OtherFlight}|{PetResource}|4|2|1" },
            await _harness.RowsAsync($"SELECT CONCAT(FlightId, '|', ResourceId, '|', CountUnit, '|', TotalCapacity, '|', Status) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId} ORDER BY FlightId"));

        await ActivateCountAsync(fixture, first.Id, 1);
        await _harness.RefusedAsync(16614, 409, fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 9)));
        await _harness.RequestAsync(fixture, scope => scope.RetireFlightCountInventory.RetireAsync(new TestFlightCountLifecycleCommand(first.Id, 2)));

        var replacement = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 6);

        Assert.Equal(
            new[] { $"{first.Id}|4|4", $"{replacement.Id}|1|6" },
            await _harness.RowsAsync($"SELECT CONCAT(Id, '|', Status, '|', TotalCapacity) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId} AND FlightId = {Flight} ORDER BY Id"));

        var raced = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(index => (Func<Task>)(() => CountAsync(fixture, airlineId, 90001, MealResource, InventoryCountUnit.Item, index))));

        Assert.Equal(1, raced.Succeeded);
        Assert.Equal(new[] { 16614 }, raced.Codes);
        Assert.Equal("1", (await _harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId} AND FlightId = 90001")).Single());
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));

        var listed = await _harness.RequestAsync(fixture, scope => scope.GetFlightCountInventoriesPaginated.ExecuteAsync(
            new BackofficeGetFlightCountInventoriesPaginatedQuery { ResourceId = PetResource, PageSize = 50 }));

        Assert.Equal(new[] { first.Id.ToString(), replacement.Id.ToString(), second.Id.ToString() }, listed.Results.Select(row => row.Id));
        Assert.Equal(
            new[] { "Airline", "Flight", "Resource", "Unit", "Total", "Closed", "Status", "Version", "Adjustments", "Updated" },
            listed.Metadata.Fields.Select(field => field.Title));
    }

    [Fact]
    public async Task P2_F02_pet_products_share_one_physical_flight_count_key()
    {
        var (fixture, airlineId, supplierId) = await ConnectedAsync();
        var cabin = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);
        var small = await _harness.ProductAsync(airlineId, supplierId, "PET_SMALL_DOG", PricingUnit.PerItem);

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", cabin.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));
        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PET_SMALL_DOG", small.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));

        var source = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 2);

        await ActivateCountAsync(fixture, source.Id, 1);

        var first = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight);
        var second = await _harness.SnapshotAsync(fixture, "PET_SMALL_DOG", Flight);

        Assert.Equal(("ConfiguredNotGuaranteed", 2, false, "NoAllocationLedger", "FlightCount"), (first.State.Name, first.ConfiguredCount!.Value, first.IsGuaranteed, first.ReasonCode, first.ResourceKind!.Name));
        Assert.Equal(first.Resource, second.Resource);
        Assert.Equal((Flight, PetResource), (first.Resource!.FlightId!.Value, first.Resource.ResourceId!.Value));
        Assert.Equal((first.ConfiguredCount, first.State), (second.ConfiguredCount, second.State));
        Assert.NotEqual(first.PolicyId, second.PolicyId);
        Assert.Equal("1", (await _harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}")).Single());
        await _harness.RefusedAsync(16614, 409, fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 2)));

        await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(new TestAdjustFlightCountInventoryCommand(source.Id, 3, "EXTRA_CARRIER", "corr-1", 2)));

        Assert.Equal((3, 3), ((await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight)).ConfiguredCount!.Value, (await _harness.SnapshotAsync(fixture, "PET_SMALL_DOG", Flight)).ConfiguredCount!.Value));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", OtherFlight)).State.Name);
    }

    [Fact]
    public async Task P2_F09_meal_variants_share_one_catering_quota()
    {
        var (fixture, airlineId, supplierId) = await ConnectedAsync();
        var child = await _harness.ProductAsync(airlineId, supplierId, "MEAL_CHML");
        var vegetarian = await _harness.ProductAsync(airlineId, supplierId, "MEAL_VGML");
        var meals = Count(MealResource, 1, InventoryCountUnit.Item);

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "MEAL_CHML", child.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: meals));

        var source = await CountAsync(fixture, airlineId, Flight, MealResource, InventoryCountUnit.Item, 30);

        await ActivateCountAsync(fixture, source.Id, 1);
        await _harness.RefusedAsync(16618, 409, fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, OtherFlight, MealResource, InventoryCountUnit.Person, 30)));

        var wrongUnit = await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "MEAL_VGML", vegetarian.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count(MealResource, 1, InventoryCountUnit.Person))));

        await _harness.RefusedAsync(16618, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(wrongUnit.Id, 1)));

        var corrected = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(wrongUnit.Id, vegetarian.Id, InventoryAuthority.Local, 1, LocalInventoryPattern.FlightCount, CountConsumption: meals)));

        await _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(wrongUnit.Id, corrected.Version)));

        var childSnapshot = await _harness.SnapshotAsync(fixture, "MEAL_CHML", Flight);
        var vegetarianSnapshot = await _harness.SnapshotAsync(fixture, "MEAL_VGML", Flight);

        Assert.Equal((30, 30), (childSnapshot.ConfiguredCount!.Value, vegetarianSnapshot.ConfiguredCount!.Value));
        Assert.Equal(childSnapshot.Resource, vegetarianSnapshot.Resource);
        Assert.Equal(
            new[] { $"{source.Id}|30" },
            await _harness.RowsAsync($"SELECT CONCAT(Id, '|', TotalCapacity) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId} AND ResourceId = {MealResource}"));
    }

    [Fact]
    public async Task P2_F06_F07_an_adjustment_is_an_audited_absolute_change_that_refuses_a_stale_version_and_replays_idempotently()
    {
        var (fixture, airlineId, _) = await ConnectedAsync();
        var source = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 8);

        _clock.Now = _clock.Now.AddMinutes(10);

        var adjusted = await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "corr-1", 1)));
        var detail = await _harness.RequestAsync(fixture, scope => scope.GetFlightCountInventoryById.ExecuteAsync(source.Id));
        var line = Assert.Single(detail.Adjustments);

        Assert.Equal((source.Id, 8, 5, 2L), (adjusted.InventoryId, adjusted.PreviousTotal, adjusted.NewTotal, adjusted.Version));
        Assert.Equal((5, 2L, "Draft", _clock.Now), (detail.TotalCapacity, detail.Version, detail.Status.Name, detail.UpdatedAt));
        Assert.Equal(
            (adjusted.AdjustmentId, 8, 5, "AIRCRAFT_CHANGE", 42L, "corr-1", _clock.Now, 1L, 2L),
            (line.Id, line.PreviousTotal, line.NewTotal, line.ReasonCode, line.ActorId, line.CorrelationId, line.OccurredAt, line.ExpectedVersion, line.ResultingVersion));
        await _harness.RefusedAsync(16605, 409, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 7, "CORRECTION", "corr-2", 1)));
        await _harness.RefusedAsync(16605, 409, fixture, scope => scope.CloseFlightCountInventoryForSale.CloseAsync(new TestFlightCountLifecycleCommand(source.Id, 1)));

        _clock.Now = _clock.Now.AddMinutes(10);

        var replayed = await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "corr-1", 1)));

        Assert.Equal(adjusted, replayed);
        await _harness.RefusedAsync(16616, 409, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 6, "AIRCRAFT_CHANGE", "corr-1", 1)));
        await _harness.RefusedAsync(16616, 409, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "corr-1", 2)));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, -1, "CORRECTION", "corr-3", 2)));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 5, "CORRECTION", "corr-3", 2)));
        await _harness.RefusedAsync(16611, 404, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(999_999_999, 5, "CORRECTION", "corr-3", 1)));

        var anonymous = InventoryFixture.Connected(airlineId);

        anonymous.ActorId = 0;
        await _harness.RefusedAsync(16610, 403, anonymous, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 7, "CORRECTION", "corr-4", 2)));
        Assert.Equal(
            new[] { "5|2|1|1" },
            await _harness.RowsAsync($"SELECT CONCAT(TotalCapacity, '|', Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments WHERE FlightCountInventoryId = {source.Id}), '|', (SELECT COUNT(*) FROM ReadModel.FlightCountAdjustments WHERE FlightCountInventoryId = {source.Id})) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {source.Id}"));
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_X01_same_expected_version_capacity_adjustment_has_one_winner_sql()
    {
        var (fixture, airlineId, _) = await ConnectedAsync();
        var source = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 10);
        var scopes = Enumerable.Range(0, 100).Select(_ => new InventoryScope(_database, _clock, fixture)).ToList();

        foreach (var scope in scopes)
            Assert.Equal(1L, (await scope.FlightCountInventories.GetAsync(source.Id))!.Version);

        var raced = await _harness.RaceAsync(scopes.Select((scope, index) => (Func<Task>)(() => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source.Id, 100 + index, "RACE", $"race-{index}", 1)))));

        foreach (var scope in scopes)
            await scope.DisposeAsync();

        Assert.Equal(1, raced.Succeeded);
        Assert.Equal(new[] { 16605 }, raced.Codes);

        var stored = (await _harness.RowsAsync($"""
            SELECT CONCAT(inventory.Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments AS line WHERE line.FlightCountInventoryId = inventory.Id), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightCountAdjustments AS line WHERE line.FlightCountInventoryId = inventory.Id), '|',
                CASE WHEN inventory.TotalCapacity = (SELECT MAX(line.NewTotal) FROM Ancillary.FlightCountAdjustments AS line WHERE line.FlightCountInventoryId = inventory.Id) THEN 'same' ELSE 'different' END, '|',
                CASE WHEN inventory.TotalCapacity BETWEEN 100 AND 199 THEN 'winner' ELSE 'other' END) AS Value
            FROM Ancillary.FlightCountInventories AS inventory WHERE inventory.Id = {source.Id}
            """)).Single();

        Assert.Equal("2|1|1|same|winner", stored);
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_W03_same_expected_version_weight_adjustment_has_one_winner_and_exact_precision_sql()
    {
        var (fixture, airlineId, _) = await ConnectedAsync();
        var source = await WeightAsync(fixture, airlineId, Flight, 100m);
        var scopes = Enumerable.Range(0, 100).Select(_ => new InventoryScope(_database, _clock, fixture)).ToList();

        foreach (var scope in scopes)
            await scope.FlightWeightInventories.GetAsync(source.Id);

        var raced = await _harness.RaceAsync(scopes.Select((scope, index) => (Func<Task>)(() => scope.AdjustFlightWeightInventory.AdjustAsync(
            new TestAdjustFlightWeightInventoryCommand(source.Id, 50.001m + (index * 0.001m), "RACE", $"race-{index}", 1)))));

        foreach (var scope in scopes)
            await scope.DisposeAsync();

        Assert.Equal(1, raced.Succeeded);
        Assert.Equal(new[] { 16605 }, raced.Codes);

        var detail = await _harness.RequestAsync(fixture, scope => scope.GetFlightWeightInventoryById.ExecuteAsync(source.Id));
        var line = Assert.Single(detail.Adjustments);

        Assert.Equal((100.000m, detail.CapacityKg, 2L), (line.PreviousKg, line.NewKg, detail.Version));
        Assert.InRange(detail.CapacityKg, 50.001m, 50.100m);
        Assert.Equal(detail.CapacityKg, decimal.Round(detail.CapacityKg, 3));
        Assert.Equal(
            new[] { "decimal|18|3", "decimal|18|3", "decimal|18|3" },
            await _harness.RowsAsync($"SELECT CONCAT(DATA_TYPE, '|', NUMERIC_PRECISION, '|', NUMERIC_SCALE) AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Ancillary' AND ((TABLE_NAME = 'FlightWeightInventories' AND COLUMN_NAME = 'CapacityKg') OR (TABLE_NAME = 'FlightWeightAdjustments' AND COLUMN_NAME IN ('PreviousKg', 'NewKg')))"));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.AdjustFlightWeightInventory.AdjustAsync(
            new TestAdjustFlightWeightInventoryCommand(source.Id, 12.3456m, "CORRECTION", "corr-precision", 2)));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.DefineFlightWeightInventory.DefineAsync(
            new TestDefineFlightWeightInventoryCommand(airlineId, OtherFlight, BagWeightResource, -0.001m)));
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_C05_F08_zero_closed_for_sale_and_suspended_are_distinct_states_and_never_unlimited()
    {
        var (fixture, airlineId, supplierId) = await ConnectedAsync();
        var meal = await _harness.ProductAsync(airlineId, supplierId, "MEAL_CHML");

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "MEAL_CHML", meal.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count(MealResource, 1, InventoryCountUnit.Item)));

        var source = await CountAsync(fixture, airlineId, Flight, MealResource, InventoryCountUnit.Item, 30);

        Assert.Equal(("NotConfigured", "SourceNotConfigured"), ((await _harness.SnapshotAsync(fixture, "MEAL_CHML", Flight)).State.Name, (await _harness.SnapshotAsync(fixture, "MEAL_CHML", Flight)).ReasonCode));
        await ActivateCountAsync(fixture, source.Id, 1);

        async Task<(string State, int? Count, bool? Closed, string? Reason)> ReadAsync(long? flightId = Flight)
        {
            var snapshot = await _harness.SnapshotAsync(fixture, "MEAL_CHML", flightId);

            Assert.False(snapshot.IsGuaranteed);

            return (snapshot.State.Name, snapshot.ConfiguredCount, snapshot.ClosedForSale, snapshot.ReasonCode);
        }

        Assert.Equal(("ConfiguredNotGuaranteed", 30, false, "NoAllocationLedger"), await ReadAsync());

        await _harness.RequestAsync(fixture, scope => scope.CloseFlightCountInventoryForSale.CloseAsync(new TestFlightCountLifecycleCommand(source.Id, 2)));
        Assert.Equal(("ClosedForSale", 30, true, "SourceClosedForSale"), await ReadAsync());

        await _harness.RequestAsync(fixture, scope => scope.OpenFlightCountInventoryForSale.OpenAsync(new TestFlightCountLifecycleCommand(source.Id, 3)));
        await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(new TestAdjustFlightCountInventoryCommand(source.Id, 0, "CATERING_CANCELLED", "corr-zero", 4)));
        Assert.Equal(("ConfiguredNotGuaranteed", 0, false, "NoAllocationLedger"), await ReadAsync());

        await _harness.RequestAsync(fixture, scope => scope.SuspendFlightCountInventory.SuspendAsync(new TestFlightCountLifecycleCommand(source.Id, 5)));
        Assert.Equal(("ClosedForSale", 0, true, "SourceClosedForSale"), await ReadAsync());
        Assert.Equal(("NotConfigured", null, null, "SourceNotConfigured"), await ReadAsync(OtherFlight));
        Assert.Equal(("Unknown", null, null, "FlightRequired"), await ReadAsync(null));
        Assert.Equal(
            new[] { $"{source.Id}|0|0|3|6|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Id, '|', TotalCapacity, '|', ClosedForSale, '|', Status, '|', Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments WHERE FlightCountInventoryId = {source.Id})) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {source.Id}"));
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_W01_fixed_5kg_bundle_uses_5kg_in_binding_even_when_price_per_item()
    {
        var (fixture, airlineId, supplierId) = await ConnectedAsync();
        var bundle = await _harness.ProductAsync(airlineId, supplierId, "XBAG_WEIGHT_5KG", PricingUnit.PerItem);
        var policyId = await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "XBAG_WEIGHT_5KG", bundle.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, WeightConsumption: FixedKg(5m)));
        var source = await WeightAsync(fixture, airlineId, Flight, 100.5m);

        await ActivateWeightAsync(fixture, source.Id, 1);

        var policy = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(policyId));
        var snapshot = await _harness.SnapshotAsync(fixture, "XBAG_WEIGHT_5KG", Flight);

        Assert.Equal(("FixedKgPerAcceptedUnit", (decimal?)5m), (policy.WeightConsumption!.ConsumptionMode.Name, policy.WeightConsumption.FixedKgPerUnit));
        Assert.Equal(("ConfiguredNotGuaranteed", 100.5m, "FlightWeight", "FlightWeight", false), (snapshot.State.Name, snapshot.ConfiguredKg!.Value, snapshot.ResourceKind!.Name, snapshot.Pattern!.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Equal((Flight, BagWeightResource), (snapshot.Resource!.FlightId!.Value, snapshot.Resource.WeightResourceId!.Value));
        Assert.Equal(
            new[] { "CapacityKg", "ClosedForSale", "CreatedAt", "FlightId", "Id", "LastUpdatedBy", "LastUpdateTime", "OwnerAirlineId", "RowVersion", "Status", "UpdatedAt", "Version", "WeightResourceId" },
            await _harness.RowsAsync($"SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Ancillary' AND TABLE_NAME = 'FlightWeightInventories' ORDER BY COLUMN_NAME"));
    }

    [Fact]
    public async Task P2_W04_count_and_weight_binding_is_closed_two_resource_contract()
    {
        var (fixture, airlineId, supplierId) = await ConnectedAsync();
        var bike = await _harness.ProductAsync(airlineId, supplierId, "SPORT_BIKE", PricingUnit.PerPiece);

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId,
            "SPORT_BIKE",
            bike.Id,
            InventoryAuthority.Local,
            LocalInventoryPattern.FlightCountPlusWeight,
            CountConsumption: Count(OversizeResource, 1, InventoryCountUnit.Equipment),
            WeightConsumption: FixedKg(12m)));

        var count = await CountAsync(fixture, airlineId, Flight, OversizeResource, InventoryCountUnit.Equipment, 3);

        await ActivateCountAsync(fixture, count.Id, 1);

        var half = await _harness.SnapshotAsync(fixture, "SPORT_BIKE", Flight);

        Assert.Equal(("NotConfigured", "SourceNotConfigured"), (half.State.Name, half.ReasonCode));
        Assert.Null(half.ConfiguredCount);

        var weight = await WeightAsync(fixture, airlineId, Flight, 120m);

        await ActivateWeightAsync(fixture, weight.Id, 1);

        var both = await _harness.SnapshotAsync(fixture, "SPORT_BIKE", Flight);

        Assert.Equal(("ConfiguredNotGuaranteed", 3, 120m, "FlightCountPlusWeight", false), (both.State.Name, both.ConfiguredCount!.Value, both.ConfiguredKg!.Value, both.Pattern!.Name, both.IsGuaranteed));
        Assert.Null(both.ResourceKind);
        Assert.Equal((Flight, OversizeResource, BagWeightResource), (both.Resource!.FlightId!.Value, both.Resource.ResourceId!.Value, both.Resource.WeightResourceId!.Value));

        await _harness.RequestAsync(fixture, scope => scope.CloseFlightWeightInventoryForSale.CloseAsync(new TestFlightWeightLifecycleCommand(weight.Id, 2)));

        var closed = await _harness.SnapshotAsync(fixture, "SPORT_BIKE", Flight);

        Assert.Equal(("ClosedForSale", 3, 120m, true), (closed.State.Name, closed.ConfiguredCount!.Value, closed.ConfiguredKg!.Value, closed.ClosedForSale!.Value));
        Assert.Equal(
            new[] { "CountPerAcceptedUnit", "CountResourceId", "CountUnit", "SlotFacilityId", "SlotOccupancyMinutes", "SlotPeoplePerAcceptedUnit", "WeightConsumptionMode", "WeightFixedKgPerUnit", "WeightResourceId" },
            await _harness.RowsAsync($"SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Ancillary' AND TABLE_NAME = 'AncillaryInventoryPolicies' AND (COLUMN_NAME LIKE 'Count%' OR COLUMN_NAME LIKE 'Weight%' OR COLUMN_NAME LIKE 'Slot%') ORDER BY COLUMN_NAME"));
    }

    [Fact]
    public async Task P2_F04_a_source_stays_draft_until_its_flight_and_resource_are_verified_by_their_source_of_truth()
    {
        var (fixture, airlineId, _) = await _harness.OperatorAsync(connected: false);
        var count = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 4);
        var weight = await WeightAsync(fixture, airlineId, Flight, 100m);

        await _harness.RefusedAsync(16608, 409, fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(count.Id, 1)));
        await _harness.RefusedAsync(16608, 409, fixture, scope => scope.ActivateFlightWeightInventory.ActivateAsync(new TestFlightWeightLifecycleCommand(weight.Id, 1)));

        fixture.Flights = [];
        fixture.Resources = [];
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(count.Id, 1)));

        fixture.Flights.Add(Flight);
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(count.Id, 1)));
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateFlightWeightInventory.ActivateAsync(new TestFlightWeightLifecycleCommand(weight.Id, 1)));

        fixture.Resources.Add((InventoryResourceKind.FlightCount, PetResource));
        fixture.Resources.Add((InventoryResourceKind.FlightWeight, BagWeightResource));

        Assert.Equal(InventoryRecordStatus.Active, (await ActivateCountAsync(fixture, count.Id, 1)).Status);
        Assert.Equal(InventoryRecordStatus.Active, (await ActivateWeightAsync(fixture, weight.Id, 1)).Status);
        Assert.Equal(
            new[] { "2|2" },
            await _harness.RowsAsync($"SELECT CONCAT((SELECT Status FROM Ancillary.FlightCountInventories WHERE Id = {count.Id}), '|', (SELECT Status FROM ReadModel.FlightWeightInventories WHERE Id = {weight.Id})) AS Value"));
    }

    [Fact]
    public async Task P2_X01_a_lost_projection_is_healed_by_replaying_the_same_correlation_without_a_second_adjustment()
    {
        var (fixture, airlineId, _) = await ConnectedAsync();
        var source = await CountAsync(fixture, airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 8);
        var command = new TestAdjustFlightCountInventoryCommand(source.Id, 5, "AIRCRAFT_CHANGE", "corr-lost", 1);

        await using (var scope = new InventoryScope(_database, _clock, fixture))
        {
            var service = new AdjustFlightCountInventoryService(
                scope.FlightCountInventories,
                new FlightCountInventoryQueryDbSynchronizer(scope.Query, _clock),
                new CommandOnlyUnitOfWork(scope.Command),
                new InventoryCallerScope(fixture, fixture),
                _database.Ids,
                _clock);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdjustAsync(command));
        }

        Assert.Equal(
            new[] { "5|2|1|8|1|0" },
            await _harness.RowsAsync($"SELECT CONCAT(command.TotalCapacity, '|', command.Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments WHERE FlightCountInventoryId = command.Id), '|', model.TotalCapacity, '|', model.Version, '|', (SELECT COUNT(*) FROM ReadModel.FlightCountAdjustments WHERE FlightCountInventoryId = command.Id)) AS Value FROM Ancillary.FlightCountInventories AS command JOIN ReadModel.FlightCountInventories AS model ON model.Id = command.Id WHERE command.Id = {source.Id}"));
        Assert.NotEmpty(await _harness.SourceDifferencesAsync(airlineId));

        var replayed = await _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(command));

        Assert.Equal((8, 5, 2L), (replayed.PreviousTotal, replayed.NewTotal, replayed.Version));
        Assert.Equal(
            new[] { "5|2|1|5|2|1" },
            await _harness.RowsAsync($"SELECT CONCAT(command.TotalCapacity, '|', command.Version, '|', (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments WHERE FlightCountInventoryId = command.Id), '|', model.TotalCapacity, '|', model.Version, '|', (SELECT COUNT(*) FROM ReadModel.FlightCountAdjustments WHERE FlightCountInventoryId = command.Id)) AS Value FROM Ancillary.FlightCountInventories AS command JOIN ReadModel.FlightCountInventories AS model ON model.Id = command.Id WHERE command.Id = {source.Id}"));
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_X03_one_hundred_sources_of_distinct_flights_are_all_accepted_concurrently()
    {
        var (fixture, airlineId, _) = await ConnectedAsync();
        var raced = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(index => (Func<Task>)(() => CountAsync(fixture, airlineId, 70000 + index, PetResource, InventoryCountUnit.AnimalCarrier, 4))));

        Assert.Equal(100, raced.Succeeded);
        Assert.Empty(raced.Codes);
        Assert.Equal(
            new[] { "100|100|100" },
            await _harness.RowsAsync($"SELECT CONCAT(COUNT(*), '|', COUNT(DISTINCT FlightId), '|', (SELECT COUNT(*) FROM ReadModel.FlightCountInventories WHERE OwnerAirlineId = {airlineId})) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}"));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, 70500, PetResource, InventoryCountUnit.AnimalCarrier, -1)));
        Assert.Equal("100", (await _harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}")).Single());
    }

    [Fact]
    public async Task P2_F10_C06_the_provision_keeps_the_purchase_cutoff_and_the_commercial_outcome_whatever_the_capacity_says()
    {
        var (fixture, airlineId, supplierId) = await ConnectedAsync();
        var meal = await _harness.ProductAsync(airlineId, supplierId, "MEAL_PREORDER_HOT");
        var paid = await _harness.Proof.RuleAsync(
            Provision(meal.Id, 100) with { AdvancePurchase = new(24, AeroTech.Messages.AirPrice.Enums.TimeUnit.Hours) },
            provisionId => V12Commands.Pricing(provisionId, Eur, V12Commands.Base(12m)));
        var blocked = await _harness.Proof.RuleAsync(
            Provision(meal.Id, 10, CommercialDisposition.NotAvailable) with { FlightApplication = new(AllowedFlightIds: [OtherFlight]) });

        await ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "MEAL_PREORDER_HOT", meal.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count(MealResource, 1, InventoryCountUnit.Item)));

        foreach (var flightId in new[] { Flight, OtherFlight })
        {
            var source = await CountAsync(fixture, airlineId, flightId, MealResource, InventoryCountUnit.Item, 30);

            await ActivateCountAsync(fixture, source.Id, 1);
        }

        var snapshot = await _harness.SnapshotAsync(fixture, "MEAL_PREORDER_HOT", OtherFlight);

        await using var reader = new AncillaryScope(_database, _clock);
        var provision = await reader.GetProvisionById.ExecuteAsync(paid.Provision.Id);
        var denied = await reader.GetProvisionById.ExecuteAsync(blocked.Provision.Id);

        Assert.Equal(("ConfiguredNotGuaranteed", 30), (snapshot.State.Name, snapshot.ConfiguredCount!.Value));
        Assert.Equal((24, "Hours", "Active", "Paid"), (provision.AdvancePurchase!.MinimumPeriod, provision.AdvancePurchase.Unit.Name, provision.Status.Name, provision.Disposition.Name));
        Assert.Equal(("NotAvailable", "Active"), (denied.Disposition.Name, denied.Status.Name));
        Assert.Equal("Active", (await reader.GetPricingById.ExecuteAsync(paid.Pricing!.Id)).Status.Name);
        Assert.Equal(
            new[] { "ClosedForSale", "CountUnit", "CreatedAt", "FlightId", "Id", "LastUpdatedBy", "LastUpdateTime", "OwnerAirlineId", "ResourceId", "RowVersion", "Status", "TotalCapacity", "UpdatedAt", "Version" },
            await _harness.RowsAsync($"SELECT COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Ancillary' AND TABLE_NAME = 'FlightCountInventories' ORDER BY COLUMN_NAME"));
    }

    private sealed class CommandOnlyUnitOfWork : IUnitOfWork
    {
        private readonly Persistence.AncillaryDbContext _command;

        public CommandOnlyUnitOfWork(Persistence.AncillaryDbContext command) => _command = command;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _command.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException("The read model was not saved.");
        }
    }
}
