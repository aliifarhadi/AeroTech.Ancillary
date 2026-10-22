using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public interface IHoldSeatSelectionsService
    {
        Task<AncillaryHoldResult> HoldAsync(IHoldSeatSelectionsCommand command, CancellationToken cancellationToken = default);
    }
}
