namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public interface IHoldAncillaryServicesCommand
    {
        string IdempotencyKey { get; }

        long OrderId { get; }

        string Reference { get; }

        DateTimeOffset? RequestedExpiresAt { get; }

        IReadOnlyList<AncillaryHoldServiceInput> Services { get; }
    }
}
