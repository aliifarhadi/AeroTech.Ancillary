using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class Reservations
{
    public static readonly DateTimeOffset Now = Quotes.AsOf;

    public static readonly DateTimeOffset ExpiresAt = Now.AddMinutes(30);

    public static ServiceReservationUnitArgs Bag(string unitReference = "U1")
        => new(unitReference, SubCodes.AirlineId, "XBAG1", 1, 501, "T1", "B1", null, [100], 1, RuleSpec.Currency, 38.50m, AncillaryInventoryControl.Unlimited);

    public static ServiceReservationUnitArgs Lounge(string unitReference = "U2")
        => new(unitReference, SubCodes.AirlineId, "LNGTHR", 1, 502, "T1", "B1", "F1", [100], 1, RuleSpec.Currency, 20.00m, AncillaryInventoryControl.Unlimited);

    public static ServiceReservation Held(params ServiceReservationUnitArgs[] units)
        => ServiceReservation.Reserve(9400, "reserve-hold:9300", "order:9001", ExpiresAt, units.Length == 0 ? [Bag()] : units, new SequentialIdGenerator(), Now);

    public static ServiceReservationUnitSelection Selection(ServiceReservationUnitArgs unit)
        => new(unit.UnitReference, unit.ProductRef, unit.ProductVersion, unit.PriceRuleId, unit.TravellerRef, unit.BoundRef, unit.FlightRef, unit.Quantity);

    public static long[] UnitIds(ServiceReservation reservation) => reservation.Units.OrderBy(unit => unit.Id).Select(unit => unit.Id).ToArray();

    public static ServiceReservationUnitStatus[] Statuses(ServiceReservation reservation, DateTimeOffset now)
        => reservation.Units.OrderBy(unit => unit.Id).Select(unit => reservation.StatusOf(unit, now)).ToArray();
}
