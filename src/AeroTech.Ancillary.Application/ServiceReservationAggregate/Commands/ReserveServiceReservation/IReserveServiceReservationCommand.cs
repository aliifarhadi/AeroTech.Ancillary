namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public interface IReserveServiceReservationCommand
    {
        string IdempotencyKey { get; }

        string Reference { get; }

        DateTimeOffset ExpiresAt { get; }

        ReservationContextInput Context { get; }

        IReadOnlyList<ReservationUnitInput> Units { get; }
    }
}
