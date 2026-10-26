using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed record AncillaryHoldUnitReceipt(
        long UnitId,
        long OrderServiceId,
        AncillaryReservationUnitStatus Status);
}
