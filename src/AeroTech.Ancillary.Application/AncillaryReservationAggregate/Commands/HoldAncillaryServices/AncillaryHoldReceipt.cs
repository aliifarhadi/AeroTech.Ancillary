using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed record AncillaryHoldReceipt(
        long HoldId,
        string Reference,
        AncillaryReservationStatus Status,
        DateTimeOffset? ExpiresAt,
        IReadOnlyList<AncillaryHoldUnitReceipt> Units);
}
