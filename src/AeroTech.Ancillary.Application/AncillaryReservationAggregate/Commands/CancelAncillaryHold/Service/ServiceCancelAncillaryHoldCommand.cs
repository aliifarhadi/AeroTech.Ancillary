using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold.Service
{
    public sealed record ServiceCancelAncillaryHoldCommand(long HoldId, string ReasonCode) : IRequest<AncillaryHoldResult>, ICancelAncillaryHoldCommand;
}
