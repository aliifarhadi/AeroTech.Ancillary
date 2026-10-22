using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    internal static class AncillaryHoldResultFactory
    {
        public static AncillaryHoldResult ToResult(this AncillaryReservation reservation, DateTimeOffset now)
            => new(
                reservation.Id,
                reservation.IdempotencyKey,
                reservation.OrderId,
                reservation.Reference,
                reservation.EffectiveStatus(now),
                reservation.ExpiresAt,
                reservation.Units
                    .OrderBy(unit => unit.Id)
                    .Select(unit => new AncillaryHoldUnitResult(
                        unit.Id,
                        unit.OrderServiceId,
                        unit.ServiceDefinitionId,
                        unit.ProvisionId,
                        unit.TravellerId,
                        unit.CoverageScope,
                        unit.CoveredFlightIds,
                        unit.Quantity,
                        unit.SeatMapSelectionId,
                        unit.SeatNumber,
                        unit.PricingId,
                        reservation.StatusOf(unit, now),
                        unit.CancellationReasonCode))
                    .ToList());
    }
}
