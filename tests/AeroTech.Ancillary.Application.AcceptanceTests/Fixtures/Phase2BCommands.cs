using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation.Service;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class Phase2BCommands
{
    public const long FlightId = 100;

    public static ReservationContextInput Context(int airlineId, DateTimeOffset asOf, params ReservationExistingOccurrenceInput[] existing)
        => new(
            Phase1Commands.CurrencyId,
            asOf,
            null,
            [new ReservationTravellerInput("T1", "ADT", ["F1"])],
            [new ReservationBoundInput("B1", [Flight("F1", FlightId, airlineId)])],
            existing);

    public static ServiceReserveServiceReservationCommand Reserve(
        string idempotencyKey,
        DateTimeOffset expiresAt,
        ReservationContextInput context,
        params ReservationUnitInput[] units)
        => new(idempotencyKey, "order:9001", expiresAt, context, units);

    public static ReservationUnitInput BagUnit(long priceRuleId, string unitReference = "U1", int quantity = 1)
        => new(unitReference, "XBAG1", 1, priceRuleId, "T1", "B1", null, quantity);

    public static ReservationUnitInput GenericBagUnit(long priceRuleId, int quantity, string unitReference = "U1")
        => new(unitReference, "XBAGG", 1, priceRuleId, "T1", "B1", null, quantity);

    public static ReservationUnitInput LoungeUnit(long priceRuleId, string unitReference = "U2")
        => new(unitReference, "LNGTHR", 1, priceRuleId, "T1", null, "F1", 1);

    public static ReservationExistingOccurrenceInput Existing(string productRef, int quantity = 1)
        => new(productRef, "T1", "B1", null, quantity);

    public static ReservationFlightInput Flight(string reference, long flightId, int airlineId)
        => new(reference, flightId, null, 1, 2, Phase1Commands.Instant("2026-10-10T08:00:00+03:30"), airlineId, airlineId, null, null, null);
}
