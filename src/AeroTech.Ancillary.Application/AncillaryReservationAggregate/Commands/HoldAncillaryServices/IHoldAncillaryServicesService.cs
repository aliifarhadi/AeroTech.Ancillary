using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public interface IHoldAncillaryServicesService
    {
        Task<AncillaryHoldResult> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default);

        Task<AncillaryHoldResult> HoldAsync(
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            CancellationToken cancellationToken = default);

        Task<AncillaryHoldResult> AppendSeatsAsync(
            long holdId,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> seats,
            CancellationToken cancellationToken = default);

        Task<AncillaryHoldResult?> FindHeldSeatsAsync(
            long? holdId,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationSeatSelectionArgs> seats,
            CancellationToken cancellationToken = default);
    }
}
