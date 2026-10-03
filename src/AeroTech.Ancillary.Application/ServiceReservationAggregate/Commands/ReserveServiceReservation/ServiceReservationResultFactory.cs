using AeroTech.Ancillary.Domain.ServiceReservationAggregate;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    internal static class ServiceReservationResultFactory
    {
        public static ServiceReservationResult ToResult(this ServiceReservation reservation, DateTimeOffset now)
            => new(
                reservation.Id,
                reservation.IdempotencyKey,
                reservation.Reference,
                reservation.ExpiresAt,
                reservation.Units
                    .OrderBy(unit => unit.Id)
                    .Select(unit => new ServiceReservationUnitResult(
                        unit.Id,
                        unit.UnitReference,
                        reservation.StatusOf(unit, now),
                        unit.OwnerAirlineId,
                        unit.ProductRef,
                        unit.ProductVersion,
                        unit.PriceRuleId,
                        unit.TravellerRef,
                        unit.BoundRef,
                        unit.FlightRef,
                        unit.CoveredFlightIds,
                        unit.Quantity,
                        unit.CurrencyId,
                        unit.Total,
                        unit.InventoryControl))
                    .ToList());
    }
}
