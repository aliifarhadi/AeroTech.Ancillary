using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class SeatReservationConformanceTests
{
    private const string SeatId = "QURULTF8MzAxfDkwMXwzQQ";

    private static AncillaryReservationUnitArgs Seat(long orderServiceId = 9001, long travellerId = 71, string seatNumber = "3a", long pricingId = 901, string seatMapSelectionId = SeatId)
        => Unit(orderServiceId: orderServiceId, travellerId: travellerId) with { SeatMapSelectionId = seatMapSelectionId, SeatNumber = seatNumber, PricingId = pricingId };

    private static AncillaryReservation Hold(params AncillaryReservationUnitArgs[] units)
        => AncillaryReservation.Hold(8001, "IDEMP-1", 7001, "ORD-7001", Now.AddMinutes(30), units.Length == 0 ? [Seat()] : units, new SequentialIdGenerator(), Now);

    [Fact]
    public void SEAT_D01_a_seat_unit_stores_its_selection_id_seat_number_and_pricing_and_replays_only_on_the_same_seat()
    {
        var reservation = Hold();
        var unit = Assert.Single(reservation.Units);

        Assert.Equal((SeatId, "3A", 901L), (unit.SeatMapSelectionId, unit.SeatNumber, unit.PricingId));
        reservation.EnsureSameContent(7001, "ORD-7001", Now.AddMinutes(30), [Seat(seatNumber: "3A")]);
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", Now.AddMinutes(30), [Seat(seatNumber: "3B", seatMapSelectionId: "other")]));
        BusinessAssert.Throws(16403, 409, () => reservation.EnsureSameContent(7001, "ORD-7001", Now.AddMinutes(30), [Seat(pricingId: 902)]));
    }

    [Fact]
    public void SEAT_D02_seat_facts_come_together_and_the_same_seat_of_a_flight_is_held_once()
    {
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { SeatNumber = "3A" }));
        BusinessAssert.Throws(16402, 422, () => Hold(Unit() with { SeatMapSelectionId = SeatId, SeatNumber = "3A" }));
        BusinessAssert.Throws(16402, 422, () => Hold(Seat(), Seat(orderServiceId: 9002, travellerId: 72, seatMapSelectionId: "other")));
        Assert.Equal(2, Hold(Seat(), Seat(orderServiceId: 9002, travellerId: 72, seatNumber: "3B", seatMapSelectionId: "other")).Units.Count);
    }

    [Fact]
    public void SEAT_D03_release_frees_a_held_hold_once_and_refuses_the_other_states()
    {
        var held = Hold();

        Assert.True(held.Release(Now));
        Assert.False(held.Release(Now));
        Assert.Equal(AncillaryReservationStatus.Released, held.Status);
        Assert.All(held.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Released, unit.Status));
        BusinessAssert.Throws(16412, 409, () => held.Confirm(Now));
        BusinessAssert.Throws(16412, 409, () => held.Cancel("CHANGE", Now));

        var confirmed = Hold();

        confirmed.Confirm(Now);
        BusinessAssert.Throws(16411, 409, () => confirmed.Release(Now));
        BusinessAssert.Throws(16407, 409, () => Hold().Release(Now.AddMinutes(31)));
    }

    [Fact]
    public void SEAT_D04_cancel_ends_a_confirmed_hold_with_a_reason_once_and_refuses_a_held_one()
    {
        var reservation = Hold();

        BusinessAssert.Throws(16414, 409, () => reservation.Cancel("CHANGE", Now));
        reservation.Confirm(Now);
        BusinessAssert.Throws(16402, 422, () => reservation.Cancel("", Now));
        Assert.True(reservation.Cancel("CHANGE", Now.AddMinutes(5)));
        Assert.False(reservation.Cancel("CHANGE", Now.AddMinutes(6)));
        Assert.Equal(AncillaryReservationStatus.Cancelled, reservation.Status);
        Assert.All(reservation.Units, unit => Assert.Equal((AncillaryReservationUnitStatus.Cancelled, "CHANGE"), (unit.Status, unit.CancellationReasonCode)));
        BusinessAssert.Throws(16413, 409, () => reservation.Confirm(Now.AddMinutes(7)));
        BusinessAssert.Throws(16413, 409, () => reservation.Release(Now.AddMinutes(7)));
    }

    [Fact]
    public void SEAT_D05_expire_persists_the_lazy_expiry_once_and_only_for_a_held_hold_past_its_deadline()
    {
        var reservation = Hold();

        Assert.False(reservation.Expire(Now.AddMinutes(29)));
        Assert.Equal(AncillaryReservationStatus.Held, reservation.Status);
        Assert.True(reservation.Expire(Now.AddMinutes(30)));
        Assert.False(reservation.Expire(Now.AddMinutes(31)));
        Assert.Equal(AncillaryReservationStatus.Expired, reservation.Status);
        Assert.All(reservation.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Expired, unit.Status));
        BusinessAssert.Throws(16407, 409, () => reservation.Confirm(Now.AddMinutes(31)));

        var confirmed = Hold();

        confirmed.Confirm(Now);
        Assert.False(confirmed.Expire(Now.AddMinutes(31)));
        Assert.Equal(AncillaryReservationStatus.Confirmed, confirmed.Status);
    }
}
