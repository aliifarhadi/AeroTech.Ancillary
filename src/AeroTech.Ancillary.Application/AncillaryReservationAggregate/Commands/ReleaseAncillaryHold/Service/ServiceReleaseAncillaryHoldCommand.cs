using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold.Service
{
    public sealed record ServiceReleaseAncillaryHoldCommand(long HoldId) : IRequest<AncillaryHoldResult>, IReleaseAncillaryHoldCommand;
}
