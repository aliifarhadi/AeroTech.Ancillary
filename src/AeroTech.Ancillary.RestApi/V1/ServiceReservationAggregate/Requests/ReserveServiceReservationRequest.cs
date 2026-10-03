using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;

namespace AeroTech.Ancillary.RestApi.V1.ServiceReservationAggregate.Requests
{
    public sealed record ReserveServiceReservationRequest(
        string IdempotencyKey,
        string Reference,
        DateTimeOffset ExpiresAt,
        ReservationContextInput Context,
        IReadOnlyList<ReservationUnitInput> Units);
}
