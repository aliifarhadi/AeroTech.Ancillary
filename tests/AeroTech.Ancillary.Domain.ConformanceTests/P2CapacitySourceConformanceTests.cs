using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Entities;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Entities;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P2Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class P2CapacitySourceConformanceTests
{
    private static readonly string[] OperationalTerms = ["Held", "Confirmed", "Sold", "Reserved", "Available", "Remaining", "Guaranteed", "Payload", "Safety", "Balance"];

    [Fact]
    public void P2_F01_F03_a_flight_count_source_is_keyed_by_owner_flight_and_resource_and_starts_as_a_draft()
    {
        var first = CountSource();
        var second = CountSource(2, Flight + 1, id: 7002);

        Assert.Equal(
            (Airline, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 4, false, InventoryRecordStatus.Draft, 1L, Now, Now),
            (first.OwnerAirlineId, first.FlightId, first.ResourceId, first.CountUnit, first.TotalCapacity, first.ClosedForSale, first.Status, first.Version, first.CreatedAt, first.UpdatedAt));
        Assert.Empty(first.Adjustments);
        Assert.NotEqual((first.FlightId, first.TotalCapacity), (second.FlightId, second.TotalCapacity));
        Assert.Equal(
            new[] { "Adjustments", "ClosedForSale", "CountUnit", "CreatedAt", "FlightId", "OwnerAirlineId", "ResourceId", "Status", "TotalCapacity", "UpdatedAt", "Version" },
            P1Fixtures.PropertiesOf<FlightCountInventory>());
        Assert.DoesNotContain(
            typeof(FlightCountInventory).GetProperties(),
            property => property.Name is "ServiceDefinitionId" or "ServiceDefinitionRef" or "ProvisionId" or "PolicyId" or "FlightNumber");
    }

    [Fact]
    public void P2_X02_a_source_refuses_negative_capacity_and_missing_identifiers()
    {
        BusinessAssert.Throws(16612, 422, () => CountSource(-1));
        BusinessAssert.Throws(16612, 422, () => CountSource(flightId: 0));
        BusinessAssert.Throws(16612, 422, () => CountSource(resourceId: 0));
        BusinessAssert.Throws(16612, 422, () => CountSource(airline: 0));
        BusinessAssert.Throws(16612, 422, () => CountSource(unit: (InventoryCountUnit)99));
        BusinessAssert.Throws(16612, 422, () => WeightSource(-0.001m));
        BusinessAssert.Throws(16612, 422, () => WeightSource(10.0005m));
        BusinessAssert.Throws(16612, 422, () => WeightSource(1_000_000_000_000_000m));
        BusinessAssert.Throws(16612, 422, () => WeightSource(flightId: 0));
        BusinessAssert.Throws(16612, 422, () => Slot(10, 11, -1));
        BusinessAssert.Throws(16612, 422, () => Slot(10, 11, facilityId: 0));

        Assert.Equal(0, CountSource(0).TotalCapacity);
        Assert.Equal(100.125m, WeightSource(100.125m).CapacityKg);
    }

    [Fact]
    public void P2_F06_an_adjustment_sets_an_absolute_total_and_appends_one_immutable_line_with_actor_reason_and_correlation()
    {
        var ids = new SequentialIdGenerator();
        var source = CountSource(8);

        var adjustment = source.AdjustTo(5, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now.AddMinutes(5));

        Assert.Equal((5, 2L, Now.AddMinutes(5)), (source.TotalCapacity, source.Version, source.UpdatedAt));
        Assert.Equal(
            (source.Id, 8, 5, "AIRCRAFT_CHANGE", Actor, "corr-1", Now.AddMinutes(5), 1L, 2L),
            (adjustment.FlightCountInventoryId, adjustment.PreviousTotal, adjustment.NewTotal, adjustment.ReasonCode, adjustment.ActorId, adjustment.CorrelationId, adjustment.OccurredAt, adjustment.ExpectedVersion, adjustment.ResultingVersion));
        Assert.Same(adjustment, Assert.Single(source.Adjustments));
        Assert.All(
            typeof(FlightCountAdjustment).GetProperties().Where(property => property.DeclaringType == typeof(FlightCountAdjustment)),
            property => Assert.False(property.SetMethod?.IsPublic == true));
        Assert.DoesNotContain(typeof(FlightCountAdjustment).GetMethods().Where(method => method.DeclaringType == typeof(FlightCountAdjustment)), method => method.IsPublic && !method.IsSpecialName);
        Assert.DoesNotContain(typeof(FlightCountInventory).GetMethods(), method => method.IsPublic && method.Name is "Change" or "SetTotal" or "Update");

        source.AdjustTo(9, "CORRECTION", Actor, "corr-2", 2, ids, Now.AddMinutes(6));

        Assert.Equal(new[] { (8, 5), (5, 9) }, source.Adjustments.Select(line => (line.PreviousTotal, line.NewTotal)));
        Assert.Equal(2, source.Adjustments.Select(line => line.Id).Distinct().Count());
    }

    [Fact]
    public void P2_F07_X01_a_stale_expected_version_is_a_conflict_and_changes_nothing()
    {
        var ids = new SequentialIdGenerator();
        var source = CountSource(8);

        source.AdjustTo(5, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now);

        BusinessAssert.Throws(16605, 409, () => source.AdjustTo(7, "CORRECTION", Actor, "corr-2", 1, ids, Now));
        BusinessAssert.Throws(16605, 409, () => source.AdjustTo(7, "CORRECTION", Actor, "corr-2", 3, ids, Now));
        BusinessAssert.Throws(16605, 409, () => source.CloseForSale(1, Now));
        BusinessAssert.Throws(16605, 409, () => source.Activate(Verified(), 1, Now));
        Assert.Equal((5, 2L, 1), (source.TotalCapacity, source.Version, source.Adjustments.Count));
    }

    [Fact]
    public void P2_X01_a_replayed_correlation_returns_the_first_adjustment_and_a_different_payload_conflicts()
    {
        var ids = new SequentialIdGenerator();
        var source = CountSource(8);
        var first = source.AdjustTo(5, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now);

        var replay = source.AdjustTo(5, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now.AddMinutes(9));

        Assert.Same(first, replay);
        Assert.Equal((5, 2L, 1, Now), (source.TotalCapacity, source.Version, source.Adjustments.Count, source.UpdatedAt));
        BusinessAssert.Throws(16616, 409, () => source.AdjustTo(6, "AIRCRAFT_CHANGE", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16616, 409, () => source.AdjustTo(5, "OTHER_REASON", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16616, 409, () => source.AdjustTo(5, "AIRCRAFT_CHANGE", Actor, "corr-1", 2, ids, Now));
        BusinessAssert.Throws(16616, 409, () => source.AdjustTo(5, "AIRCRAFT_CHANGE", Actor + 1, "corr-1", 1, ids, Now));
        Assert.Single(source.Adjustments);
    }

    [Fact]
    public void P2_X02_an_adjustment_needs_a_real_change_a_reason_an_actor_and_a_correlation()
    {
        var ids = new SequentialIdGenerator();
        var source = CountSource(8);

        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(-1, "CORRECTION", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(8, "CORRECTION", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(5, " ", Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(5, new string('R', 51), Actor, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(5, "CORRECTION", 0, "corr-1", 1, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(5, "CORRECTION", Actor, "", 1, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(5, "CORRECTION", Actor, new string('c', 65), 1, ids, Now));
        Assert.Equal((8, 1L), (source.TotalCapacity, source.Version));
        Assert.Empty(source.Adjustments);
    }

    [Fact]
    public void P2_C05_F08_zero_capacity_closed_for_sale_and_suspended_are_three_separate_states_that_keep_the_total()
    {
        var ids = new SequentialIdGenerator();
        var source = CountSource(30, resourceId: MealResource, unit: InventoryCountUnit.Item);

        source.Activate(Verified(), 1, Now);
        source.CloseForSale(2, Now.AddMinutes(1));

        Assert.Equal((true, 30, InventoryRecordStatus.Active, 3L), (source.ClosedForSale, source.TotalCapacity, source.Status, source.Version));
        BusinessAssert.Throws(16613, 409, () => source.CloseForSale(3, Now));

        source.OpenForSale(3, Now.AddMinutes(2));
        source.Suspend(4, Now.AddMinutes(3));

        Assert.Equal((false, 30, InventoryRecordStatus.Suspended, 5L), (source.ClosedForSale, source.TotalCapacity, source.Status, source.Version));
        BusinessAssert.Throws(16613, 409, () => source.Suspend(5, Now));
        BusinessAssert.Throws(16613, 409, () => source.OpenForSale(5, Now));

        source.AdjustTo(0, "CATERING_CANCELLED", Actor, "corr-zero", 5, ids, Now.AddMinutes(4));

        Assert.Equal((0, false, InventoryRecordStatus.Suspended), (source.TotalCapacity, source.ClosedForSale, source.Status));

        source.Activate(Verified(InventoryCountUnit.Item), 6, Now.AddMinutes(5));
        source.Retire(7, Now.AddMinutes(6));

        Assert.Equal((InventoryRecordStatus.Retired, 8L, 0, 1), (source.Status, source.Version, source.TotalCapacity, source.Adjustments.Count));
        BusinessAssert.Throws(16613, 409, () => source.AdjustTo(3, "CORRECTION", Actor, "corr-late", 8, ids, Now));
        BusinessAssert.Throws(16613, 409, () => source.Activate(Verified(), 8, Now));
        BusinessAssert.Throws(16613, 409, () => source.CloseForSale(8, Now));
        BusinessAssert.Throws(16613, 409, () => source.Retire(8, Now));
    }

    [Fact]
    public void P2_F04_a_source_is_activated_only_against_a_verified_flight_and_resource_with_one_count_unit_per_resource()
    {
        var source = CountSource();

        BusinessAssert.Throws(16608, 409, () => source.Activate(new FlightSourceEvidence(InventoryReferenceCheck.SourceUnavailable, InventoryReferenceCheck.Verified, null), 1, Now));
        BusinessAssert.Throws(16608, 409, () => source.Activate(new FlightSourceEvidence(InventoryReferenceCheck.Verified, InventoryReferenceCheck.SourceUnavailable, null), 1, Now));
        BusinessAssert.Throws(16609, 422, () => source.Activate(new FlightSourceEvidence(InventoryReferenceCheck.NotFound, InventoryReferenceCheck.Verified, null), 1, Now));
        BusinessAssert.Throws(16609, 422, () => source.Activate(new FlightSourceEvidence(InventoryReferenceCheck.Verified, InventoryReferenceCheck.NotFound, null), 1, Now));
        BusinessAssert.Throws(16618, 409, () => source.Activate(Verified(InventoryCountUnit.Piece), 1, Now));
        Assert.Equal((InventoryRecordStatus.Draft, 1L), (source.Status, source.Version));

        source.Activate(Verified(InventoryCountUnit.AnimalCarrier), 1, Now);

        Assert.Equal((InventoryRecordStatus.Active, 2L), (source.Status, source.Version));
        BusinessAssert.Throws(16613, 409, () => source.Activate(Verified(), 2, Now));
    }

    [Fact]
    public void P2_W03_W05_a_weight_source_is_commercial_kilograms_with_three_decimals_and_makes_no_load_control_claim()
    {
        var ids = new SequentialIdGenerator();
        var source = WeightSource(100m);

        Assert.Equal(
            new[] { "Adjustments", "CapacityKg", "ClosedForSale", "CreatedAt", "FlightId", "OwnerAirlineId", "Status", "UpdatedAt", "Version", "WeightResourceId" },
            P1Fixtures.PropertiesOf<FlightWeightInventory>());
        Assert.Equal(typeof(decimal), typeof(FlightWeightInventory).GetProperty(nameof(FlightWeightInventory.CapacityKg))!.PropertyType);

        var adjustment = source.AdjustTo(87.125m, "COMMERCIAL_ALLOWANCE", Actor, "corr-1", 1, ids, Now);

        Assert.Equal((100m, 87.125m, 87.125m, 2L), (adjustment.PreviousKg, adjustment.NewKg, source.CapacityKg, source.Version));
        Assert.Equal(source.Id, adjustment.FlightWeightInventoryId);
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(87.1255m, "CORRECTION", Actor, "corr-2", 2, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(-1m, "CORRECTION", Actor, "corr-2", 2, ids, Now));
        BusinessAssert.Throws(16612, 422, () => source.AdjustTo(87.125m, "CORRECTION", Actor, "corr-2", 2, ids, Now));
        BusinessAssert.Throws(16605, 409, () => source.AdjustTo(50m, "CORRECTION", Actor, "corr-2", 1, ids, Now));

        source.Activate(Verified(), 2, Now);

        Assert.Equal(InventoryRecordStatus.Active, source.Status);
        Assert.DoesNotContain(
            new[] { typeof(FlightWeightInventory), typeof(FlightWeightAdjustment), typeof(FlightCountInventory), typeof(AirportSlotInventory) }.SelectMany(type => type.GetProperties()),
            property => OperationalTerms.Any(term => property.Name.Contains(term, StringComparison.Ordinal)));
    }

    [Fact]
    public void P2_A01_A02_an_airport_slot_is_a_bounded_utc_interval_of_one_facility()
    {
        var slot = Slot(10, 10, 20, toMinute: 30);
        var otherFacility = Slot(10, 10, 12, Lounge + 1, 7202, toMinute: 30);

        Assert.Equal(
            (Airline, Ika, Lounge, new DateTimeOffset(2027, 4, 6, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2027, 4, 6, 10, 30, 0, TimeSpan.Zero), 20, InventoryRecordStatus.Draft, 1L),
            (slot.OwnerAirlineId, slot.AirportId, slot.FacilityId, slot.StartUtc, slot.EndUtc, slot.CapacityPersons, slot.Status, slot.Version));
        Assert.Equal(
            new[] { "Adjustments", "AirportId", "CapacityPersons", "ClosedForSale", "CreatedAt", "EndUtc", "FacilityId", "OwnerAirlineId", "StartUtc", "Status", "UpdatedAt", "Version" },
            P1Fixtures.PropertiesOf<AirportSlotInventory>());
        Assert.NotEqual(slot.FacilityId, otherFacility.FacilityId);
        Assert.False(otherFacility.Overlaps(slot.OwnerAirlineId, slot.FacilityId, slot.StartUtc, slot.EndUtc));
        BusinessAssert.Throws(16612, 422, () => Slot(11, 10));
        BusinessAssert.Throws(16612, 422, () => Slot(10, 10));
        BusinessAssert.Throws(16612, 422, () => AirportSlotInventory.Define(1, Airline, 0, Lounge, slot.StartUtc, slot.EndUtc, 1, Now));
        BusinessAssert.Throws(16612, 422, () => AirportSlotInventory.Define(1, Airline, Ika, Lounge, slot.StartUtc, slot.StartUtc.AddHours(24).AddMinutes(1), 1, Now));
        BusinessAssert.Throws(16612, 422, () => AirportSlotInventory.Define(1, Airline, Ika, Lounge, slot.StartUtc.AddSeconds(30), slot.EndUtc, 1, Now));
    }

    [Fact]
    public void P2_A04_A05_overlapping_intervals_of_one_facility_conflict_even_when_not_identical_and_adjacent_ones_do_not()
    {
        var slot = Slot(10, 11);
        DateTimeOffset At(int hour, int minute = 0) => new(2027, 4, 6, hour, minute, 0, TimeSpan.Zero);

        Assert.True(slot.Overlaps(Airline, Lounge, At(10), At(11)));
        Assert.True(slot.Overlaps(Airline, Lounge, At(10, 30), At(11, 30)));
        Assert.True(slot.Overlaps(Airline, Lounge, At(9, 30), At(10, 1)));
        Assert.True(slot.Overlaps(Airline, Lounge, At(10, 15), At(10, 45)));
        Assert.True(slot.Overlaps(Airline, Lounge, At(9), At(12)));
        Assert.False(slot.Overlaps(Airline, Lounge, At(11), At(12)));
        Assert.False(slot.Overlaps(Airline, Lounge, At(9), At(10)));
        Assert.False(slot.Overlaps(Airline, Lounge + 1, At(10), At(11)));
        Assert.False(slot.Overlaps(Airline + 1, Lounge, At(10), At(11)));

        slot.Retire(1, Now);

        Assert.False(slot.Overlaps(Airline, Lounge, At(10), At(11)));
    }

    [Fact]
    public void P2_A03_an_occupancy_of_ninety_minutes_intersects_every_slot_it_touches_and_not_only_the_arrival_slot()
    {
        var binding = AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.ValueObjects.AirportSlotConsumption.Create(Lounge, 90, 1);
        var slots = Enumerable.Range(0, 6)
            .Select(index => AirportSlotInventory.Define(
                7300 + index,
                Airline,
                Ika,
                Lounge,
                new DateTimeOffset(2027, 4, 6, 10, 0, 0, TimeSpan.Zero).AddMinutes(index * 30),
                new DateTimeOffset(2027, 4, 6, 10, 30, 0, TimeSpan.Zero).AddMinutes(index * 30),
                20,
                Now))
            .ToList();
        var arrival = new DateTimeOffset(2027, 4, 6, 10, 30, 0, TimeSpan.Zero);
        var touched = slots.Where(slot => slot.Overlaps(Airline, Lounge, arrival, arrival.AddMinutes(binding.OccupancyMinutes))).Select(slot => slot.StartUtc.TimeOfDay).ToArray();

        Assert.Equal(new[] { new TimeSpan(10, 30, 0), new TimeSpan(11, 0, 0), new TimeSpan(11, 30, 0) }, touched);
        Assert.Equal(
            new[] { new TimeSpan(10, 30, 0), new TimeSpan(11, 0, 0), new TimeSpan(11, 30, 0), new TimeSpan(12, 0, 0) },
            slots.Where(slot => slot.Overlaps(Airline, Lounge, arrival.AddMinutes(15), arrival.AddMinutes(15 + binding.OccupancyMinutes))).Select(slot => slot.StartUtc.TimeOfDay));
        Assert.DoesNotContain(typeof(AirportSlotInventory).GetMethods(), method => method.Name is "Occupy" or "Consume" or "Reserve");
    }

    [Fact]
    public void P2_A06_dst_local_time_must_have_authoritative_timezone_and_utc_resolution()
    {
        var localAmbiguous = new DateTime(2027, 10, 31, 2, 30, 0);
        var firstInstant = new DateTimeOffset(localAmbiguous, TimeSpan.FromHours(2));
        var secondInstant = new DateTimeOffset(localAmbiguous, TimeSpan.FromHours(1));

        BusinessAssert.Throws(16612, 422, () => AirportSlotInventory.Define(1, Airline, Ika, Lounge, firstInstant, firstInstant.AddMinutes(30), 20, Now));

        var first = AirportSlotInventory.Define(1, Airline, Ika, Lounge, firstInstant.ToUniversalTime(), firstInstant.ToUniversalTime().AddMinutes(30), 20, Now);
        var second = AirportSlotInventory.Define(2, Airline, Ika, Lounge, secondInstant.ToUniversalTime(), secondInstant.ToUniversalTime().AddMinutes(30), 20, Now);

        Assert.Equal(TimeSpan.Zero, first.StartUtc.Offset);
        Assert.Equal(TimeSpan.FromHours(1), second.StartUtc - first.StartUtc);
        Assert.False(first.Overlaps(Airline, Lounge, second.StartUtc, second.EndUtc));
        BusinessAssert.Throws(16608, 409, () => first.Activate(new AirportSlotEvidence(true, InventoryReferenceCheck.SourceUnavailable, null, null), 1, Now));
        BusinessAssert.Throws(16609, 422, () => first.Activate(new AirportSlotEvidence(false, InventoryReferenceCheck.Verified, Ika, "Europe/Paris"), 1, Now));
        BusinessAssert.Throws(16609, 422, () => first.Activate(new AirportSlotEvidence(true, InventoryReferenceCheck.NotFound, null, null), 1, Now));
        BusinessAssert.Throws(16609, 422, () => first.Activate(SlotVerified(Ika + 1), 1, Now));
        BusinessAssert.Throws(16609, 422, () => first.Activate(SlotVerified(timeZoneId: " "), 1, Now));
        BusinessAssert.Throws(16609, 422, () => first.Activate(SlotVerified(timeZoneId: "Not/AZone"), 1, Now));

        first.Activate(SlotVerified(timeZoneId: "Europe/Paris"), 1, Now);

        Assert.Equal((InventoryRecordStatus.Active, 2L), (first.Status, first.Version));
    }

    [Fact]
    public void P2_A08_a_slot_is_adjusted_closed_and_suspended_like_every_other_source()
    {
        var ids = new SequentialIdGenerator();
        var slot = Slot(10, 11, 20);
        var neighbour = Slot(10, 11, 15, Lounge + 1, 7202);

        var adjustment = slot.AdjustTo(0, "MAINTENANCE", Actor, "corr-1", 1, ids, Now);

        Assert.Equal((slot.Id, 20, 0, 2L), (adjustment.AirportSlotInventoryId, adjustment.PreviousTotal, adjustment.NewTotal, adjustment.ResultingVersion));
        Assert.Equal((15, false, 1L), (neighbour.CapacityPersons, neighbour.ClosedForSale, neighbour.Version));
        Assert.Same(adjustment, slot.AdjustTo(0, "MAINTENANCE", Actor, "corr-1", 1, ids, Now));

        slot.Activate(SlotVerified(), 2, Now);
        slot.CloseForSale(3, Now);

        Assert.Equal((true, 0, InventoryRecordStatus.Active), (slot.ClosedForSale, slot.CapacityPersons, slot.Status));
        Assert.All(
            typeof(AirportSlotAdjustment).GetProperties().Where(property => property.DeclaringType == typeof(AirportSlotAdjustment)),
            property => Assert.False(property.SetMethod?.IsPublic == true));
    }
}
