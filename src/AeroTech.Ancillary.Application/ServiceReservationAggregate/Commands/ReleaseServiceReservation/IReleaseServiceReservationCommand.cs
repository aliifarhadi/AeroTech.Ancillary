namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation
{
    public interface IReleaseServiceReservationCommand
    {
        long ServiceReservationId { get; }
    }
}
