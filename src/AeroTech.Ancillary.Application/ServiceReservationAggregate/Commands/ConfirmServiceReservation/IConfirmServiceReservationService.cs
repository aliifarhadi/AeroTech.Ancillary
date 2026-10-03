using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation
{
    public interface IConfirmServiceReservationService
    {
        Task<ServiceReservationResult> ConfirmAsync(IConfirmServiceReservationCommand command, CancellationToken cancellationToken = default);
    }
}
