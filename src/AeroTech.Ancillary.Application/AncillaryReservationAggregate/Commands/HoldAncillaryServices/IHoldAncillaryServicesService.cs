using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public interface IHoldAncillaryServicesService
    {
        Task<AncillaryHoldReceipt> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default);

        Task<AncillaryHoldReceipt> HoldAsync(
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            CancellationToken cancellationToken = default);

        Task<AncillaryHoldReceipt> AppendSeatsAsync(
            long holdId,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationUnitArgs> seats,
            CancellationToken cancellationToken = default);

        Task<AncillaryHoldReceipt?> FindHeldSeatsAsync(
            long? holdId,
            AncillaryHoldHeader header,
            IReadOnlyList<AncillaryReservationSeatSelectionArgs> seats,
            CancellationToken cancellationToken = default);
    }
}
