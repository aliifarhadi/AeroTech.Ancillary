using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service
{
    public sealed record ServiceHoldAncillaryServicesCommand(
        string IdempotencyKey,
        long OrderId,
        string Reference,
        DateTimeOffset? RequestedExpiresAt,
        IReadOnlyList<AncillaryHoldServiceInput> Services) : IRequest<AncillaryHoldResult>, IHoldAncillaryServicesCommand;
}
