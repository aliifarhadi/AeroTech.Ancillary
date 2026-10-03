namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation
{
    public interface IConfirmServiceReservationCommand
    {
        long ServiceReservationId { get; }
    }
}
