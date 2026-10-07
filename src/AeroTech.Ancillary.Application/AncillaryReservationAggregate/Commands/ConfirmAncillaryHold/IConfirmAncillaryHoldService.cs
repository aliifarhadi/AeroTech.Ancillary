using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold
{
    public interface IConfirmAncillaryHoldService
    {
        Task<AncillaryHoldResult> ConfirmAsync(IConfirmAncillaryHoldCommand command, CancellationToken cancellationToken = default);
    }
}
