using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold.Service
{
    public sealed record ServiceConfirmAncillaryHoldCommand(long HoldId)
        : IRequest<AncillaryHoldResult>, IConfirmAncillaryHoldCommand;
}
