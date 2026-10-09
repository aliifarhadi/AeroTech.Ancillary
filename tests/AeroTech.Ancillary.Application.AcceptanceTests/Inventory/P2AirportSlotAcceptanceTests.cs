using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application._Shared.Time;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

[Collection(DatabaseCollection.Name)]
public class P2AirportSlotAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public P2AirportSlotAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private Task<AirportSlotInventoryResult> SlotAsync(InventoryFixture fixture, int airlineId, long facilityId, DateTimeOffset startUtc, DateTimeOffset endUtc, int capacity)
        => _harness.RequestAsync(fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, facilityId, startUtc, endUtc, capacity)));

    private Task<AirportSlotInventoryResult> ActivateAsync(InventoryFixture fixture, long inventoryId, long expectedVersion)
        => _harness.RequestAsync(fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(inventoryId, expectedVersion)));

    private async Task<(InventoryFixture Fixture, int AirlineId, long SupplierId)> AirportAsync()
    {
        var connected = await _harness.OperatorAsync();

        connected.Fixture.Airports.Add(Ika);
        connected.Fixture.Facilities![Lounge] = (Ika, "Asia/Tehran");
        connected.Fixture.Facilities[SecondLounge] = (Ika, "Asia/Tehran");
        connected.Fixture.CountingFamilies!.Add("LOUNGE");

        return connected;
    }

    private async Task<string> OverlapsAsync(int airlineId)
        => (await _harness.RowsAsync($"""
            SELECT CONCAT(
                (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId} AND Status <> 4), '|',
                (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories AS first
                 JOIN Ancillary.AirportSlotInventories AS second
                   ON second.OwnerAirlineId = first.OwnerAirlineId AND second.FacilityId = first.FacilityId AND second.Id > first.Id
                  AND second.Status <> 4 AND first.StartUtc < second.EndUtc AND second.StartUtc < first.EndUtc
                 WHERE first.OwnerAirlineId = {airlineId} AND first.Status <> 4)) AS Value
            """)).Single();

    [Fact]
    public async Task P2_A01_A02_A05_adjacent_utc_slots_are_accepted()
    {
        var (fixture, airlineId, _) = await AirportAsync();
        var first = await SlotAsync(fixture, airlineId, Lounge, Utc(10), Utc(10, 30), 20);
        var second = await SlotAsync(fixture, airlineId, Lounge, Utc(10, 30), Utc(11), 20);
        var before = await SlotAsync(fixture, airlineId, Lounge, Utc(9, 30), Utc(10), 20);
        var otherFacility = await SlotAsync(fixture, airlineId, SecondLounge, Utc(10), Utc(10, 30), 12);

        Assert.Equal(
            (InventoryRecordStatus.Draft, 1L, Ika, Lounge, Utc(10), Utc(10, 30), 20),
            (first.Status, first.Version, first.AirportId, first.FacilityId, first.StartUtc, first.EndUtc, first.CapacityPersons));
        Assert.Equal(4, new[] { first.Id, second.Id, before.Id, otherFacility.Id }.Distinct().Count());
        Assert.Equal("4|0", await OverlapsAsync(airlineId));
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, 999, Lounge, Utc(12), Utc(12, 30), 20)));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, new DateTimeOffset(2027, 4, 6, 15, 0, 0, TimeSpan.FromMinutes(210)), Utc(12, 30), 20)));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, Utc(13), Utc(12), 20)));
        await _harness.RefusedAsync(16612, 422, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, Utc(12), Utc(12, 30), -1)));

        var listed = await _harness.RequestAsync(fixture, scope => scope.GetAirportSlotInventoriesPaginated.ExecuteAsync(
            new BackofficeGetAirportSlotInventoriesPaginatedQuery { FacilityId = Lounge, FromUtc = Utc(10), ToUtc = Utc(11), PageSize = 50 }));

        Assert.Equal(new[] { first.Id.ToString(), second.Id.ToString() }, listed.Results.Select(row => row.Id));
        Assert.Equal(
            new[] { "Airline", "Airport", "Facility", "Start (UTC)", "End (UTC)", "Persons", "Closed", "Status", "Version", "Adjustments", "Updated" },
            listed.Metadata.Fields.Select(field => field.Title));
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_A04_an_interval_that_overlaps_a_current_slot_of_the_same_facility_is_refused_even_when_not_identical()
    {
        var (fixture, airlineId, _) = await AirportAsync();
        var (other, otherAirlineId, _) = await AirportAsync();
        var slot = await SlotAsync(fixture, airlineId, Lounge, Utc(10), Utc(11), 20);

        foreach (var (start, end) in new[] { (Utc(10), Utc(11)), (Utc(10, 30), Utc(11, 30)), (Utc(9, 30), Utc(10, 1)), (Utc(10, 15), Utc(10, 45)), (Utc(9), Utc(12)) })
        {
            await _harness.RefusedAsync(16615, 409, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
                new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, start, end, 20)));
        }

        Assert.Equal("1|0", await OverlapsAsync(airlineId));
        Assert.Equal(InventoryRecordStatus.Draft, (await SlotAsync(fixture, airlineId, SecondLounge, Utc(10, 30), Utc(11, 30), 12)).Status);
        Assert.Equal(InventoryRecordStatus.Draft, (await SlotAsync(other, otherAirlineId, Lounge, Utc(10, 30), Utc(11, 30), 20)).Status);

        await ActivateAsync(fixture, slot.Id, 1);
        await _harness.RefusedAsync(16615, 409, fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Ika, Lounge, Utc(10, 30), Utc(11, 30), 20)));
        await _harness.RequestAsync(fixture, scope => scope.RetireAirportSlotInventory.RetireAsync(new TestAirportSlotLifecycleCommand(slot.Id, 2)));

        var replacement = await SlotAsync(fixture, airlineId, Lounge, Utc(10, 30), Utc(11, 30), 20);

        Assert.NotEqual(slot.Id, replacement.Id);
        Assert.Equal("2|0", await OverlapsAsync(airlineId));
        Assert.Equal(
            new[] { $"{slot.Id}|4", $"{replacement.Id}|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Id, '|', Status) AS Value FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId} AND FacilityId = {Lounge} ORDER BY Id"));
    }

    [Fact]
    public async Task P2_A04_concurrent_overlapping_facility_slot_creation_has_single_winner_sql()
    {
        var (fixture, airlineId, _) = await AirportAsync();
        var overlapping = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(index => (Func<Task>)(() => SlotAsync(
            fixture,
            airlineId,
            Lounge,
            Utc(8).AddMinutes(index),
            Utc(11).AddMinutes(index),
            20))));

        Assert.Equal(1, overlapping.Succeeded);
        Assert.Equal(new[] { 16615 }, overlapping.Codes);
        Assert.Equal("1|0", await OverlapsAsync(airlineId));

        var adjacent = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(index => (Func<Task>)(() => SlotAsync(
            fixture,
            airlineId,
            SecondLounge,
            Utc(0).AddMinutes(index * 10),
            Utc(0).AddMinutes((index + 1) * 10),
            12))));

        Assert.Equal(100, adjacent.Succeeded);
        Assert.Empty(adjacent.Codes);
        Assert.Equal("101|0", await OverlapsAsync(airlineId));
        Assert.Equal(
            new[] { "101" },
            await _harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM ReadModel.AirportSlotInventories WHERE OwnerAirlineId = {airlineId}"));
        Assert.Empty(await _harness.RowsAsync($"SELECT resource_description AS Value FROM sys.dm_tran_locks WHERE resource_type = 'APPLICATION' AND resource_description LIKE {'%' + airlineId.ToString() + '%'}"));
    }

    [Fact]
    public async Task P2_A06_dst_local_time_must_have_authoritative_timezone_and_utc_resolution()
    {
        var (fixture, airlineId, _) = await AirportAsync();

        BusinessAssert.Throws(16612, 422, () => UtcInstant.Parse("2027-10-31T02:30:00", "StartUtc"));
        BusinessAssert.Throws(16612, 422, () => UtcInstant.Parse("2027-10-31 02:30", "StartUtc"));
        BusinessAssert.Throws(16612, 422, () => UtcInstant.Parse(null, "StartUtc"));
        BusinessAssert.Throws(16612, 422, () => UtcInstant.Parse("tomorrow", "StartUtc"));
        Assert.Null(UtcInstant.ParseOptional(" ", "AtUtc"));

        var firstInstant = UtcInstant.Parse("2027-10-31T02:30:00+02:00", "StartUtc");
        var secondInstant = UtcInstant.Parse("2027-10-31T02:30:00+01:00", "StartUtc");

        Assert.Equal((new DateTimeOffset(2027, 10, 31, 0, 30, 0, TimeSpan.Zero), TimeSpan.Zero), (firstInstant, firstInstant.Offset));
        Assert.Equal(TimeSpan.FromHours(1), secondInstant - firstInstant);
        Assert.Equal(Utc(10), UtcInstant.Parse("2027-04-06T10:00Z", "StartUtc"));

        fixture.Facilities![Lounge] = (Ika, "Europe/Paris");

        var first = await SlotAsync(fixture, airlineId, Lounge, firstInstant, firstInstant.AddMinutes(30), 20);
        var second = await SlotAsync(fixture, airlineId, Lounge, secondInstant, secondInstant.AddMinutes(30), 20);

        Assert.Equal("2|0", await OverlapsAsync(airlineId));

        var disconnected = new InventoryFixture(airlineId);

        disconnected.Airports.Add(Ika);
        await _harness.RefusedAsync(16608, 409, disconnected, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(first.Id, 1)));

        fixture.Facilities.Remove(Lounge);
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(first.Id, 1)));

        fixture.Facilities[Lounge] = (Ika + 1, "Europe/Paris");
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(first.Id, 1)));

        fixture.Facilities[Lounge] = (Ika, "Not/AZone");
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(first.Id, 1)));

        fixture.Facilities[Lounge] = (Ika, "Europe/Paris");

        Assert.Equal(InventoryRecordStatus.Active, (await ActivateAsync(fixture, first.Id, 1)).Status);
        Assert.Equal(InventoryRecordStatus.Active, (await ActivateAsync(fixture, second.Id, 1)).Status);
        Assert.Equal(
            new[] { "2027-10-31 00:30:00 +00:00|2027-10-31 01:00:00 +00:00", "2027-10-31 01:30:00 +00:00|2027-10-31 02:00:00 +00:00" },
            await _harness.RowsAsync($"SELECT CONCAT(CONVERT(varchar(26), StartUtc, 120), '|', CONVERT(varchar(26), EndUtc, 120)) AS Value FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId} ORDER BY StartUtc"));
    }

    [Fact]
    public async Task P2_A07_A08_a_lounge_slot_is_reported_per_facility_and_instant_beside_its_passenger_usage_limit()
    {
        var (fixture, airlineId, supplierId) = await AirportAsync();
        var lounge = await _harness.ProductAsync(airlineId, supplierId, "LOUNGE_DOTAIR_IKA", basis: ServiceDateBasis.ServiceStart);
        var second = await _harness.ProductAsync(airlineId, supplierId, "LOUNGE_CIP_IKA", basis: ServiceDateBasis.ServiceStart);

        foreach (var (reference, definitionId, facilityId) in new[] { ("LOUNGE_DOTAIR_IKA", lounge.Id, Lounge), ("LOUNGE_CIP_IKA", second.Id, SecondLounge) })
        {
            var draft = await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(new TestDefineInventoryPolicyCommand(
                airlineId,
                reference,
                definitionId,
                InventoryAuthority.Local,
                LocalInventoryPattern.AirportSlot,
                SlotConsumption: SlotUse(facilityId),
                PassengerUsageLimits: [Limit(PassengerUsageLimitScope.PerServiceDate, 1, "LOUNGE")])));

            await _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1)));
        }

        var slot = await SlotAsync(fixture, airlineId, Lounge, Utc(10), Utc(10, 30), 20);
        var neighbour = await SlotAsync(fixture, airlineId, SecondLounge, Utc(10), Utc(10, 30), 12);

        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA", atUtc: Utc(10, 15))).State.Name);
        await ActivateAsync(fixture, slot.Id, 1);
        await ActivateAsync(fixture, neighbour.Id, 1);

        var inside = await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA", atUtc: Utc(10, 15));

        Assert.Equal(("ConfiguredNotGuaranteed", 20, "AirportSlot", "AirportSlot", false), (inside.State.Name, inside.ConfiguredCount!.Value, inside.ResourceKind!.Name, inside.Pattern!.Name, inside.IsGuaranteed));
        Assert.Equal((Ika, Lounge, Utc(10), Utc(10, 30)), (inside.Resource!.AirportId!.Value, inside.Resource.FacilityId!.Value, inside.Resource.StartUtc!.Value, inside.Resource.EndUtc!.Value));
        Assert.Equal("ConfiguredNotGuaranteed", (await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA", atUtc: Utc(10))).State.Name);
        Assert.Equal(("NotConfigured", "SourceNotConfigured"), ((await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA", atUtc: Utc(10, 30))).State.Name, (await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA", atUtc: Utc(10, 30))).ReasonCode));
        Assert.Equal(("Unknown", "InstantRequired"), ((await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA")).State.Name, (await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA")).ReasonCode));

        var adjusted = await _harness.RequestAsync(fixture, scope => scope.AdjustAirportSlotInventory.AdjustAsync(new TestAdjustAirportSlotInventoryCommand(slot.Id, 18, "MAINTENANCE", "corr-1", 2)));

        await _harness.RequestAsync(fixture, scope => scope.CloseAirportSlotInventoryForSale.CloseAsync(new TestAirportSlotLifecycleCommand(slot.Id, 3)));

        var closed = await _harness.SnapshotAsync(fixture, "LOUNGE_DOTAIR_IKA", atUtc: Utc(10, 15));
        var open = await _harness.SnapshotAsync(fixture, "LOUNGE_CIP_IKA", atUtc: Utc(10, 15));
        var policy = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyByServiceIdentity.ExecuteAsync("LOUNGE_DOTAIR_IKA"));

        Assert.Equal((20, 18, 3L), (adjusted.PreviousTotal, adjusted.NewTotal, adjusted.Version));
        Assert.Equal(("ClosedForSale", 18, true), (closed.State.Name, closed.ConfiguredCount!.Value, closed.ClosedForSale!.Value));
        Assert.Equal(("ConfiguredNotGuaranteed", 12, false), (open.State.Name, open.ConfiguredCount!.Value, open.ClosedForSale!.Value));
        Assert.Equal((Lounge, 90, 1), (policy.SlotConsumption!.FacilityId, policy.SlotConsumption.OccupancyMinutes, policy.SlotConsumption.PeoplePerAcceptedUnit));
        Assert.Equal(("PerServiceDate", 1, "LOUNGE"), policy.PassengerUsageLimits.Select(limit => (limit.LimitScope.Name, limit.MaxUnits, limit.CountingFamilyCode)).Single());
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }

    [Fact]
    public async Task P2_X01_same_expected_version_slot_adjustment_has_one_winner_sql()
    {
        var (fixture, airlineId, _) = await AirportAsync();
        var slot = await SlotAsync(fixture, airlineId, Lounge, Utc(10), Utc(11), 20);
        var scopes = Enumerable.Range(0, 100).Select(_ => new InventoryScope(_database, _clock, fixture)).ToList();

        foreach (var scope in scopes)
            await scope.AirportSlotInventories.GetAsync(slot.Id);

        var raced = await _harness.RaceAsync(scopes.Select((scope, index) => (Func<Task>)(() => scope.AdjustAirportSlotInventory.AdjustAsync(
            new TestAdjustAirportSlotInventoryCommand(slot.Id, 100 + index, "RACE", $"race-{index}", 1)))));

        foreach (var scope in scopes)
            await scope.DisposeAsync();

        Assert.Equal(1, raced.Succeeded);
        Assert.Equal(new[] { 16605 }, raced.Codes);
        Assert.Equal(
            new[] { "2|1|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Version, '|', (SELECT COUNT(*) FROM Ancillary.AirportSlotAdjustments WHERE AirportSlotInventoryId = {slot.Id}), '|', (SELECT COUNT(*) FROM ReadModel.AirportSlotAdjustments WHERE AirportSlotInventoryId = {slot.Id})) AS Value FROM Ancillary.AirportSlotInventories WHERE Id = {slot.Id}"));
        Assert.Empty(await _harness.SourceDifferencesAsync(airlineId));
    }
}
