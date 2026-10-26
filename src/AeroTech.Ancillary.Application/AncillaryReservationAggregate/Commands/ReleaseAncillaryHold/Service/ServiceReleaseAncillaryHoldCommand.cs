using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold.Service
{
    public sealed record ServiceReleaseAncillaryHoldCommand(long HoldId, IReadOnlyList<long>? UnitIds) : IRequest<AncillaryHoldResult>, IReleaseAncillaryHoldCommand;
}
