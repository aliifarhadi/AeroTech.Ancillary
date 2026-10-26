using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V10ReservationLifecycleConformanceTests
{
    private static readonly DateTimeOffset ExpiresAt = Now.AddMinutes(30);

    private readonly SequentialIdGenerator _ids = new();

    private AncillaryReservation Hold(params AncillaryReservationUnitArgs[] units)
        => AncillaryReservation.Hold(8001, "IDEMP-1", 7001, "ORD-7001", ExpiresAt, units, _ids, Now);

    private AncillaryReservation ThreeServices()
        => Hold(Unit(9001, travellerId: 71), Unit(9002, travellerId: 72), Unit(9003, travellerId: 73));

    private static AncillaryReservationUnitArgs Seat(long orderServiceId, string seatNumber, string seatMapSelectionId)
        => Unit(orderServiceId, travellerId: 71) with { SeatMapSelectionId = seatMapSelectionId, SeatNumber = seatNumber, PricingId = 901 };

    private static long IdOf(AncillaryReservation reservation, long orderServiceId)
        => reservation.Units.Single(unit => unit.OrderServiceId == orderServiceId).Id;

    private static AncillaryReservationUnitStatus StatusOf(AncillaryReservation reservation, long orderServiceId)
        => reservation.Units.Single(unit => unit.OrderServiceId == orderServiceId).Status;

    [Fact]
    public void V10_L01_L08_L06_L07_a_selected_release_frees_only_the_named_units_and_the_last_one_releases_the_hold()
    {
        var reservation = ThreeServices();

        Assert.Equal(3, reservation.Units.Select(unit => unit.Id).Distinct().Count());
        Assert.True(reservation.Release([IdOf(reservation, 9002)], Now));
        Assert.Equal(
            (AncillaryReservationStatus.Held, AncillaryReservationUnitStatus.Held, AncillaryReservationUnitStatus.Released, AncillaryReservationUnitStatus.Held),
            (reservation.Status, StatusOf(reservation, 9001), StatusOf(reservation, 9002), StatusOf(reservation, 9003)));
        Assert.False(reservation.Release([IdOf(reservation, 9002)], Now));

        Assert.True(reservation.Release(null, Now.AddMinutes(1)));
        Assert.Equal(AncillaryReservationStatus.Released, reservation.Status);
        Assert.All(reservation.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Released, unit.Status));
        Assert.False(reservation.Release(null, Now.AddMinutes(2)));
        Assert.Equal(Now.AddMinutes(1), reservation.UpdatedAt);
    }

    [Fact]
    public void V10_L09_L10_L04_confirm_skips_released_units_and_a_confirmed_unit_cannot_be_released()
    {
        var reservation = Hold(Unit(9001, travellerId: 71), Unit(9002, travellerId: 72));

        reservation.Release([IdOf(reservation, 9001)], Now);

        Assert.True(reservation.Confirm(Now));
        Assert.Equal(
            (AncillaryReservationStatus.Confirmed, AncillaryReservationUnitStatus.Released, AncillaryReservationUnitStatus.Confirmed),
            (reservation.Status, StatusOf(reservation, 9001), StatusOf(reservation, 9002)));
        Assert.False(reservation.Confirm(Now));
        BusinessAssert.Throws(16411, 409, () => reservation.Release([IdOf(reservation, 9002)], Now));
        BusinessAssert.Throws(16411, 409, () => reservation.Release(null, Now));
        Assert.False(reservation.Release([IdOf(reservation, 9001)], Now));
    }

    [Fact]
    public void V10_L11_L12_L13_L14_cancel_whole_or_selected_keeps_released_units_and_earlier_reasons()
    {
        var reservation = ThreeServices();

        reservation.Release([IdOf(reservation, 9003)], Now);
        reservation.Confirm(Now);

        Assert.True(reservation.Cancel("CUSTOMER", [IdOf(reservation, 9001)], Now.AddMinutes(1)));
        Assert.Equal(
            (AncillaryReservationStatus.Confirmed, AncillaryReservationUnitStatus.Cancelled, AncillaryReservationUnitStatus.Confirmed),
            (reservation.Status, StatusOf(reservation, 9001), StatusOf(reservation, 9002)));
        Assert.False(reservation.Cancel("CUSTOMER", [IdOf(reservation, 9001)], Now.AddMinutes(2)));
        BusinessAssert.Throws(16413, 409, () => reservation.Cancel("SCHEDULE", [IdOf(reservation, 9001)], Now.AddMinutes(2)));

        Assert.True(reservation.Cancel("SCHEDULE", null, Now.AddMinutes(3)));
        Assert.Equal(AncillaryReservationStatus.Cancelled, reservation.Status);
        Assert.Equal(
            new[] { ("CUSTOMER", AncillaryReservationUnitStatus.Cancelled), ("SCHEDULE", AncillaryReservationUnitStatus.Cancelled), ((string?)null, AncillaryReservationUnitStatus.Released) }.ToList(),
            reservation.Units.OrderBy(unit => unit.OrderServiceId).Select(unit => (unit.CancellationReasonCode, unit.Status)).ToList());
        Assert.False(reservation.Cancel("OTHER", null, Now.AddMinutes(4)));
    }

    [Fact]
    public void V10_L15_a_selected_operation_is_all_or_nothing_and_refuses_units_in_the_wrong_state()
    {
        var held = Hold(Unit(9001, travellerId: 71), Unit(9002, travellerId: 72));

        BusinessAssert.Throws(16414, 409, () => held.Cancel("CUSTOMER", [IdOf(held, 9001)], Now));

        var confirmed = Hold(Unit(9001, travellerId: 71), Unit(9002, travellerId: 72));

        confirmed.Release([IdOf(confirmed, 9002)], Now);
        confirmed.Confirm(Now);

        BusinessAssert.Throws(16412, 409, () => confirmed.Cancel("CUSTOMER", [IdOf(confirmed, 9001), IdOf(confirmed, 9002)], Now));
        Assert.Equal(AncillaryReservationUnitStatus.Confirmed, StatusOf(confirmed, 9001));
        Assert.Null(confirmed.Units.Single(unit => unit.OrderServiceId == 9001).CancellationReasonCode);
    }

    [Fact]
    public void V10_unit_ids_must_be_a_non_empty_distinct_list_of_units_of_this_hold()
    {
        var reservation = ThreeServices();
        var first = IdOf(reservation, 9001);

        BusinessAssert.Throws(16402, 422, () => reservation.Release([], Now));
        BusinessAssert.Throws(16402, 422, () => reservation.Release([first, first], Now));
        BusinessAssert.Throws(16402, 422, () => reservation.Release([0], Now));
        BusinessAssert.Throws(16418, 404, () => reservation.Release([first, 999_999], Now));
        BusinessAssert.Throws(16418, 404, () => reservation.Cancel("CUSTOMER", [999_999], Now));
        Assert.All(reservation.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Held, unit.Status));
    }

    [Fact]
    public void V10_L16_L17_L18_expiry_moves_only_held_units_once_and_a_replay_comes_before_the_deadline()
    {
        var reservation = Hold(Unit(9001, travellerId: 71), Unit(9002, travellerId: 72));

        reservation.Release([IdOf(reservation, 9001)], Now);

        BusinessAssert.Throws(16407, 409, () => Hold(Unit()).Confirm(ExpiresAt));
        Assert.False(reservation.Expire(ExpiresAt.AddTicks(-1)));
        Assert.True(reservation.Expire(ExpiresAt));
        Assert.False(reservation.Expire(ExpiresAt.AddMinutes(1)));
        Assert.Equal(
            (AncillaryReservationStatus.Expired, AncillaryReservationUnitStatus.Released, AncillaryReservationUnitStatus.Expired),
            (reservation.Status, StatusOf(reservation, 9001), StatusOf(reservation, 9002)));
        Assert.False(reservation.Release([IdOf(reservation, 9001)], ExpiresAt.AddMinutes(1)));
        BusinessAssert.Throws(16407, 409, () => reservation.Release([IdOf(reservation, 9002)], ExpiresAt.AddMinutes(1)));
    }

    [Fact]
    public void V10_L19_L20_a_create_replay_compares_quantity_and_slot_facts_by_instant()
    {
        var appointment = new DateTimeOffset(2026, 11, 1, 16, 0, 0, TimeSpan.FromHours(3.5));
        var lounge = Unit(quantity: 2) with { AppointmentTimeWithOffset = appointment, GuestCount = 1 };
        var reservation = Hold(lounge);

        reservation.EnsureSameContent(7001, "ORD-7001", ExpiresAt, [lounge]);
        reservation.EnsureSameContent(7001, "ORD-7001", ExpiresAt, [lounge with { AppointmentTimeWithOffset = appointment.ToUniversalTime() }]);
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", ExpiresAt, [lounge with { GuestCount = 2 }]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", ExpiresAt, [lounge with { AppointmentTimeWithOffset = appointment.AddMinutes(30) }]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", ExpiresAt, [lounge with { Quantity = 1 }]));
    }

    [Fact]
    public void V10_A14_two_provisions_of_one_definition_for_the_same_traveller_and_flight_are_two_units()
    {
        var reservation = Hold(Unit(9001, provisionId: 501), Unit(9002, provisionId: 502));

        Assert.Equal(new long[] { 501, 502 }, reservation.Units.Select(unit => unit.ProvisionId).Order());
    }

    [Fact]
    public void V10_I27_slot_facts_and_allocations_follow_the_unit_rules()
    {
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { GuestCount = 1 }));
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { AppointmentTimeWithOffset = ExpiresAt, GuestCount = -1 }));
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { Allocations = [new(InventoryResourceKind.FlightCount, 41, 1.5m)] }));
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { Allocations = [new(InventoryResourceKind.FlightWeight, 42, 2.0005m)] }));
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { Allocations = [new(InventoryResourceKind.AirportSlot, 43, 0m)] }));
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { Allocations = [new(InventoryResourceKind.FlightCount, 41, 1m), new(InventoryResourceKind.FlightCount, 41, 2m)] }));

        var unit = Assert.Single(Hold(Unit(quantity: 2) with { Allocations = [new(InventoryResourceKind.FlightCount, 41, 2m), new(InventoryResourceKind.FlightWeight, 42, 10.5m)] }).Units);

        Assert.Equal(
            new[] { (InventoryResourceKind.FlightCount, 41L, 2m), (InventoryResourceKind.FlightWeight, 42L, 10.5m) },
            unit.Allocations.OrderBy(allocation => allocation.ResourceKind).Select(allocation => (allocation.ResourceKind, allocation.InventorySourceId, allocation.Amount)));
        Assert.All(unit.Allocations, allocation => Assert.Equal(unit.Id, allocation.AncillaryReservationUnitId));
    }

    [Fact]
    public void V10_S08_S09_a_seat_unit_is_one_sector_seat_without_facts_or_allocations()
    {
        var seat = Seat(9101, "3A", "SEAT-3A");

        BusinessAssert.Throws(16402, 422, () => Hold(seat with { Quantity = 2 }));
        BusinessAssert.Throws(16402, 422, () => Hold(seat with { CoverageScope = ServiceCoverageScope.Portion }));
        BusinessAssert.Throws(16402, 422, () => Hold(seat with { AppointmentTimeWithOffset = ExpiresAt }));
        BusinessAssert.Throws(16402, 422, () => Hold(seat with { Allocations = [new(InventoryResourceKind.FlightCount, 41, 1m)] }));
        Assert.Empty(Assert.Single(Hold(seat).Units).Allocations);
    }

    [Fact]
    public void V10_S02_S05_S06_S07_a_seat_append_extends_the_same_hold_once_per_key()
    {
        var reservation = Hold(Unit(9001));
        var seat = Seat(9101, "3A", "SEAT-3A");

        Assert.True(reservation.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat], _ids, Now.AddMinutes(1)));
        Assert.Equal((8001L, 2, AncillaryReservationStatus.Held), (reservation.Id, reservation.Units.Count, reservation.Status));
        Assert.Equal("APPEND-1", reservation.Units.Single(unit => unit.OrderServiceId == 9101).AppendIdempotencyKey);

        Assert.False(reservation.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat with { SeatNumber = "3a" }], _ids, Now.AddMinutes(2)));
        BusinessAssert.Throws(16403, 409, () => reservation.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [Seat(9101, "3B", "SEAT-3B")], _ids, Now));
        BusinessAssert.Throws(16402, 422, () => reservation.AppendSeats("APPEND-2", 7001, "ORD-7001", ExpiresAt, [Seat(9102, "3A", "SEAT-3A-2")], _ids, Now));
        BusinessAssert.Throws(16406, 409, () => reservation.AppendSeats("APPEND-3", 7001, "ORD-7001", ExpiresAt, [Seat(9001, "4A", "SEAT-4A")], _ids, Now));
        Assert.Equal(2, reservation.Units.Count);
        reservation.EnsureSameContent(7001, "ORD-7001", ExpiresAt, [Unit(9001)]);
    }

    [Fact]
    public void V10_S03_S04_an_append_refuses_another_order_reference_expiry_or_a_hold_that_is_not_held()
    {
        var seat = Seat(9101, "3A", "SEAT-3A");
        var held = Hold(Unit(9001));

        BusinessAssert.Throws(16402, 422, () => held.AppendSeats("APPEND-1", 7002, "ORD-7001", ExpiresAt, [seat], _ids, Now));
        BusinessAssert.Throws(16402, 422, () => held.AppendSeats("APPEND-1", 7001, "ORD-OTHER", ExpiresAt, [seat], _ids, Now));
        BusinessAssert.Throws(16402, 422, () => held.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt.AddMinutes(5), [seat], _ids, Now));
        BusinessAssert.Throws(16402, 422, () => held.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [Unit(9102)], _ids, Now));
        BusinessAssert.Throws(16407, 409, () => held.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat], _ids, ExpiresAt));
        Assert.Single(held.Units);

        var confirmed = Hold(Unit(9001));

        confirmed.Confirm(Now);
        BusinessAssert.Throws(16411, 409, () => confirmed.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat], _ids, Now));

        var released = Hold(Unit(9001));

        released.Release(null, Now);
        BusinessAssert.Throws(16412, 409, () => released.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat], _ids, Now));
    }

    [Fact]
    public void V10_S05_E05_a_seat_replay_is_recognised_by_its_selection_identity_and_a_changed_header_or_seat_is_a_conflict()
    {
        var reservation = Hold(Unit(9001));
        var seat = Seat(9101, "3A", "SEAT-3A");
        var selection = new AncillaryReservationSeatSelectionArgs(9101, 71, "SEAT-3A");

        Assert.False(reservation.HoldsSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [selection]));
        Assert.True(reservation.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat], _ids, Now));
        Assert.True(reservation.HoldsSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [selection]));
        Assert.False(reservation.AppendSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [seat with { PricingId = 902 }], _ids, Now));

        BusinessAssert.Throws(16403, 409, () => reservation.HoldsSeats("APPEND-1", 7002, "ORD-7001", ExpiresAt, [selection]));
        BusinessAssert.Throws(16403, 409, () => reservation.HoldsSeats("APPEND-1", 7001, "ORD-OTHER", ExpiresAt, [selection]));
        BusinessAssert.Throws(16403, 409, () => reservation.HoldsSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt.AddMinutes(1), [selection]));
        BusinessAssert.Throws(16403, 409, () => reservation.HoldsSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [selection with { TravellerId = 72 }]));
        BusinessAssert.Throws(16403, 409, () => reservation.HoldsSeats("APPEND-1", 7001, "ORD-7001", ExpiresAt, [selection, selection with { OrderServiceId = 9102, SeatMapSelectionId = "SEAT-3B" }]));
        BusinessAssert.Throws(16403, 409, () => reservation.HoldsSeats(null, 7001, "ORD-7001", ExpiresAt, [selection]));
        BusinessAssert.Throws(16403, 409, () => reservation.AppendSeats("APPEND-1", 7002, "ORD-7001", ExpiresAt, [seat], _ids, Now));
        Assert.Equal(2, reservation.Units.Count);
    }

    [Fact]
    public void V10_L22_the_status_numbers_are_unchanged()
    {
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, new[] { AncillaryReservationStatus.Held, AncillaryReservationStatus.Confirmed, AncillaryReservationStatus.Released, AncillaryReservationStatus.Expired, AncillaryReservationStatus.Cancelled }.Select(status => (int)status));
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, new[] { AncillaryReservationUnitStatus.Held, AncillaryReservationUnitStatus.Confirmed, AncillaryReservationUnitStatus.Released, AncillaryReservationUnitStatus.Expired, AncillaryReservationUnitStatus.Cancelled }.Select(status => (int)status));
    }
}
