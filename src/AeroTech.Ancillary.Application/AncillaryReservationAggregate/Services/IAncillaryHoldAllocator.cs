using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public interface IAncillaryHoldAllocator
    {
        Task<IReadOnlyList<AncillaryReservationUnitArgs>> AllocateAsync(
            IReadOnlyList<AncillaryReservationUnitArgs> units,
            CancellationToken cancellationToken = default);
    }
}
