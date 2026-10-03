using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation.Service
{
    public sealed record ServiceReserveServiceReservationCommand(
        string IdempotencyKey,
        string Reference,
        DateTimeOffset ExpiresAt,
        ReservationContextInput Context,
        IReadOnlyList<ReservationUnitInput> Units) : IRequest<ServiceReservationResult>, IReserveServiceReservationCommand;
}
