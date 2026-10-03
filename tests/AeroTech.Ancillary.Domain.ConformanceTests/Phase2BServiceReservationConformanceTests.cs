using System.Reflection;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase2BServiceReservationConformanceTests
{
    private static readonly DateTimeOffset Now = Reservations.Now;
    private static readonly DateTimeOffset ExpiresAt = Reservations.ExpiresAt;

    [Fact]
    public void P2B_V01_ReserveHoldsEveryUnitWithTheAcceptedValues()
    {
        var reservation = Reservations.Held();
        var unit = Assert.Single(reservation.Units);

        Assert.Equal((9400L, "reserve-hold:9300", "order:9001", ExpiresAt, Now), (reservation.Id, reservation.IdempotencyKey, reservation.Reference, reservation.ExpiresAt, reservation.CreatedAt));
        Assert.Equal(
            (ServiceReservationUnitStatus.Held, "U1", SubCodes.AirlineId, "XBAG1", 1, 501L, "T1", "B1", (string?)null),
            (unit.Status, unit.UnitReference, unit.OwnerAirlineId, unit.ProductRef, unit.ProductVersion, unit.PriceRuleId, unit.TravellerRef, unit.BoundRef, unit.FlightRef));
        Assert.Equal([100L], unit.CoveredFlightIds);
        Assert.Equal((1, RuleSpec.Currency, 38.50m, AncillaryInventoryControl.Unlimited), (unit.Quantity, unit.CurrencyId, unit.Total, unit.InventoryControl));
        Assert.Equal((reservation.Id, 1001L), (unit.ServiceReservationId, unit.Id));
    }

    [Fact]
    public void P2B_V03_OnlyTheSameContentRepeatsAReservation()
    {
        var bag = Reservations.Bag();
        var lounge = Reservations.Lounge();
        var reservation = Reservations.Held(bag, lounge);
        var bagSelection = Reservations.Selection(bag);
        var loungeSelection = Reservations.Selection(lounge);

        reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection, loungeSelection]);
        reservation.EnsureSameContent("order:9001", ExpiresAt, [loungeSelection with { BoundRef = null }, bagSelection]);
        reservation.EnsureSameContent("order:9001", ExpiresAt.ToOffset(TimeSpan.FromHours(3.5)), [bagSelection, loungeSelection]);

        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9002", ExpiresAt, [bagSelection, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt.AddMinutes(1), [bagSelection, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection, loungeSelection, bagSelection with { UnitReference = "U3" }]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection, bagSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { UnitReference = "U9" }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { ProductRef = "XBAGG" }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { ProductVersion = 2 }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { PriceRuleId = 502 }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { TravellerRef = "T2" }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { BoundRef = "B2" }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { BoundRef = null }, loungeSelection]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection, loungeSelection with { BoundRef = "B2" }]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection, loungeSelection with { FlightRef = "F2" }]));
        BusinessAssert.Throws(16405, 409, () => reservation.EnsureSameContent("order:9001", ExpiresAt, [bagSelection with { Quantity = 2 }, loungeSelection]));
    }

    [Fact]
    public void P2B_V10_ReservationRequestMustExpireInTheFutureAndNameDistinctUnits()
    {
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.EnsureReservable(Now, ["U1"], Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.EnsureReservable(Now.AddTicks(-1), ["U1"], Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.EnsureReservable(ExpiresAt, ["U1", "U1"], Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.EnsureReservable(ExpiresAt, [], Now));
        ServiceReservation.EnsureReservable(Now.AddTicks(1), ["U1", "u1"], Now);

        BusinessAssert.Throws(16412, 422, () => ServiceReservation.Reserve(1, "k", "r", Now, [Reservations.Bag()], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.Reserve(1, "k", "r", ExpiresAt, [Reservations.Bag(), Reservations.Lounge("U1")], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.Reserve(1, "k", "r", ExpiresAt, [], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.Reserve(1, "", "r", ExpiresAt, [Reservations.Bag()], new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16412, 422, () => ServiceReservation.Reserve(1, "k", new string('r', 129), ExpiresAt, [Reservations.Bag()], new SequentialIdGenerator(), Now));
    }

    [Fact]
    public void P2B_V13_ConfirmConfirmsEveryUnitAndRepeatingChangesNothing()
    {
        var reservation = Reservations.Held(Reservations.Bag(), Reservations.Lounge());

        Assert.True(reservation.Confirm(Now));
        Assert.Equal([ServiceReservationUnitStatus.Confirmed, ServiceReservationUnitStatus.Confirmed], Reservations.Statuses(reservation, Now));
        Assert.False(reservation.Confirm(Now));
        Assert.Equal([ServiceReservationUnitStatus.Confirmed, ServiceReservationUnitStatus.Confirmed], Reservations.Statuses(reservation, Now));
    }

    [Fact]
    public void P2B_V14_ReleaseReleasesEveryUnitAndConflictsAreRefused()
    {
        var released = Reservations.Held(Reservations.Bag(), Reservations.Lounge());

        Assert.True(released.Release(Now));
        Assert.Equal([ServiceReservationUnitStatus.Released, ServiceReservationUnitStatus.Released], Reservations.Statuses(released, Now));
        Assert.False(released.Release(Now));
        BusinessAssert.Throws(16407, 409, () => released.Confirm(Now));

        var confirmed = Reservations.Held();

        confirmed.Confirm(Now);

        BusinessAssert.Throws(16410, 409, () => confirmed.Release(Now));
        Assert.Equal([ServiceReservationUnitStatus.Confirmed], Reservations.Statuses(confirmed, Now));
    }

    [Fact]
    public void P2B_V15_HeldUnitsCountAsExpiredFromExpiresAtOnly()
    {
        var held = Reservations.Held(Reservations.Bag(), Reservations.Lounge());
        var confirmed = Reservations.Held();

        confirmed.Confirm(Now);

        Assert.Equal([ServiceReservationUnitStatus.Held, ServiceReservationUnitStatus.Held], Reservations.Statuses(held, ExpiresAt.AddTicks(-1)));
        Assert.Equal([ServiceReservationUnitStatus.Expired, ServiceReservationUnitStatus.Expired], Reservations.Statuses(held, ExpiresAt));
        Assert.All(held.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Held, unit.Status));

        BusinessAssert.Throws(16406, 409, () => held.Confirm(ExpiresAt));
        BusinessAssert.Throws(16406, 409, () => held.Release(ExpiresAt));
        BusinessAssert.Throws(16406, 409, () => held.Cancel(Reservations.UnitIds(held), ExpiresAt));
        Assert.All(held.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Held, unit.Status));

        Assert.Equal([ServiceReservationUnitStatus.Confirmed], Reservations.Statuses(confirmed, ExpiresAt.AddDays(1)));
        Assert.False(confirmed.Confirm(ExpiresAt.AddDays(1)));
    }

    [Fact]
    public void P2B_V16_CancelCancelsConfirmedUnitsOnly()
    {
        var confirmed = Reservations.Held();
        var held = Reservations.Held();
        var unitRef = Reservations.UnitIds(confirmed);

        confirmed.Confirm(Now);

        Assert.True(confirmed.Cancel(unitRef, Now));
        Assert.Equal([ServiceReservationUnitStatus.Cancelled], Reservations.Statuses(confirmed, Now));
        Assert.False(confirmed.Cancel(unitRef, Now));
        BusinessAssert.Throws(16411, 409, () => held.Cancel(Reservations.UnitIds(held), Now));
        BusinessAssert.Throws(16412, 422, () => held.Cancel([unitRef[0] + 1_000], Now));
        BusinessAssert.Throws(16412, 422, () => confirmed.Cancel([], Now));
        Assert.Equal([ServiceReservationUnitStatus.Held], Reservations.Statuses(held, Now));
    }

    [Fact]
    public void P2B_V17_CancellingOneOfTwoConfirmedUnitsLeavesTheOtherConfirmed()
    {
        var reservation = Reservations.Held(Reservations.Bag(), Reservations.Lounge());
        var units = Reservations.UnitIds(reservation);

        reservation.Confirm(Now);

        Assert.True(reservation.Cancel([units[0]], Now));
        Assert.Equal([ServiceReservationUnitStatus.Cancelled, ServiceReservationUnitStatus.Confirmed], Reservations.Statuses(reservation, Now));
        BusinessAssert.Throws(16408, 409, () => reservation.Confirm(Now));
        BusinessAssert.Throws(16410, 409, () => reservation.Release(Now));
        BusinessAssert.Throws(16409, 409, () => reservation.Cancel(units, Now));
        Assert.Equal([ServiceReservationUnitStatus.Cancelled, ServiceReservationUnitStatus.Confirmed], Reservations.Statuses(reservation, Now));
        Assert.True(reservation.Cancel([units[1]], Now));
        Assert.Equal([ServiceReservationUnitStatus.Cancelled, ServiceReservationUnitStatus.Cancelled], Reservations.Statuses(reservation, Now));
    }

    [Fact]
    public void P2B_V18_ConfirmKeepsTheAcceptedProductVersionRuleAndTotal()
    {
        var reservation = Reservations.Held(Reservations.Bag(), Reservations.Lounge());
        var before = reservation.Units.Select(unit => (unit.ProductRef, unit.ProductVersion, unit.PriceRuleId, unit.Total, unit.InventoryControl)).ToList();

        reservation.Confirm(Now);

        Assert.Equal(before, reservation.Units.Select(unit => (unit.ProductRef, unit.ProductVersion, unit.PriceRuleId, unit.Total, unit.InventoryControl)));
    }

    [Fact]
    public void P2B_V20_ReservationRaisesNoDomainEvent()
    {
        var first = Reservations.Held(Reservations.Bag(), Reservations.Lounge());
        var second = Reservations.Held();

        first.Confirm(Now);
        first.Cancel([Reservations.UnitIds(first)[0]], Now);
        second.Release(Now);

        Assert.Empty(first.GetEvents());
        Assert.Empty(second.GetEvents());
    }

    [Fact]
    public void P2B_G01_NoPhaseOneOrPhaseTwoTestIsSkipped()
    {
        var tests = typeof(Phase2BServiceReservationConformanceTests).Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.Name.StartsWith("P1_", StringComparison.Ordinal) || method.Name.StartsWith("P2_", StringComparison.Ordinal))
            .ToList();

        Assert.Contains(tests, method => method.Name.StartsWith("P1_", StringComparison.Ordinal));
        Assert.Contains(tests, method => method.Name.StartsWith("P2_", StringComparison.Ordinal));
        Assert.All(tests, method => Assert.Null(Assert.Single(method.GetCustomAttributes<FactAttribute>()).Skip));
    }
}
