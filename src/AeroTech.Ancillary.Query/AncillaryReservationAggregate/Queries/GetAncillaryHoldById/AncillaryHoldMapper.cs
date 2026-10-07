using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById
{
    public static class AncillaryHoldMapper
    {
        public static AncillaryHoldDto ToAncillaryHold(AncillaryReservation reservation, DateTimeOffset now)
            => new(
                reservation.Id,
                reservation.IdempotencyKey,
                reservation.OrderId,
                reservation.Reference,
                reservation.EffectiveStatus(now),
                reservation.ExpiresAt,
                reservation.Units
                    .OrderBy(unit => unit.Id)
                    .Select(unit => new AncillaryHoldUnitDto(
                        unit.Id,
                        unit.OrderServiceId,
                        unit.ServiceDefinitionId,
                        unit.ProvisionId,
                        unit.TravellerId,
                        unit.CoverageScope,
                        unit.CoveredFlightIds,
                        unit.Quantity,
                        reservation.StatusOf(unit, now)))
                    .ToList());
    }
}
