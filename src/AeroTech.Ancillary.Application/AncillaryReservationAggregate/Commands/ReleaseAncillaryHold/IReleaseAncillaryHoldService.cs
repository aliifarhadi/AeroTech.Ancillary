using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold
{
    public interface IReleaseAncillaryHoldService
    {
        Task<AncillaryHoldReceipt> ReleaseAsync(IReleaseAncillaryHoldCommand command, CancellationToken cancellationToken = default);
    }
}
