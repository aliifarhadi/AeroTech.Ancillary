using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold
{
    public interface ICancelAncillaryHoldService
    {
        Task<AncillaryHoldResult> CancelAsync(ICancelAncillaryHoldCommand command, CancellationToken cancellationToken = default);
    }
}
