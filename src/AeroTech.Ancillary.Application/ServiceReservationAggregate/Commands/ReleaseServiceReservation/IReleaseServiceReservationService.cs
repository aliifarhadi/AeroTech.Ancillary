using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation
{
    public interface IReleaseServiceReservationService
    {
        Task<ServiceReservationResult> ReleaseAsync(IReleaseServiceReservationCommand command, CancellationToken cancellationToken = default);
    }
}
