namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public interface IReserveServiceReservationService
    {
        Task<ServiceReservationResult> ReserveAsync(IReserveServiceReservationCommand command, CancellationToken cancellationToken = default);
    }
}
