using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits
{
    public interface ICancelServiceReservationUnitsService
    {
        Task<ServiceReservationResult> CancelAsync(ICancelServiceReservationUnitsCommand command, CancellationToken cancellationToken = default);
    }
}
