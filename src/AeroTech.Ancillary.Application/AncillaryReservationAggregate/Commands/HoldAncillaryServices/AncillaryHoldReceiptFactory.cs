using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    internal static class AncillaryHoldReceiptFactory
    {
        public static AncillaryHoldReceipt ToReceipt(this AncillaryReservation reservation, DateTimeOffset now)
            => new(
                reservation.Id,
                reservation.Reference,
                reservation.EffectiveStatus(now),
                reservation.ExpiresAt,
                reservation.Units
                    .OrderBy(unit => unit.Id)
                    .Select(unit => new AncillaryHoldUnitReceipt(unit.Id, unit.OrderServiceId, reservation.StatusOf(unit, now)))
                    .ToList());
    }
}
