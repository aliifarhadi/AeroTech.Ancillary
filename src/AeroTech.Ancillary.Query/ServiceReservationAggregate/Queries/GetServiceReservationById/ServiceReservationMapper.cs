using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Ancillary.Query.ServiceReservationAggregate.Dto;

namespace AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById
{
    public static class ServiceReservationMapper
    {
        public static ServiceReservationDto ToServiceReservation(ServiceReservation reservation, DateTimeOffset now)
            => new(
                reservation.Id,
                reservation.IdempotencyKey,
                reservation.Reference,
                reservation.ExpiresAt,
                reservation.Units
                    .OrderBy(unit => unit.Id)
                    .Select(unit => new ServiceReservationUnitDto(
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
