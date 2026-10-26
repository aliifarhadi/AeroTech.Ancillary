using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold
{
    public interface IConfirmAncillaryHoldService
    {
        Task<AncillaryHoldReceipt> ConfirmAsync(IConfirmAncillaryHoldCommand command, CancellationToken cancellationToken = default);
    }
}
