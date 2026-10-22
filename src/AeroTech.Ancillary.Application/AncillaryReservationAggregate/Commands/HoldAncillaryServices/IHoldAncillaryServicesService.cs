using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public interface IHoldAncillaryServicesService
    {
        Task<AncillaryHoldResult> HoldAsync(IHoldAncillaryServicesCommand command, CancellationToken cancellationToken = default);

        Task<AncillaryHoldResult> HoldAsync(AncillaryHoldHeader header, IReadOnlyList<AncillaryReservationUnitArgs> units, CancellationToken cancellationToken = default);
    }
}
