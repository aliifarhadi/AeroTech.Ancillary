namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed record AncillaryHoldHeader(
        string IdempotencyKey,
        long OrderId,
        string Reference,
        DateTimeOffset RequestedExpiresAt);
}
