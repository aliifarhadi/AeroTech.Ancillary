using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;

namespace AeroTech.Ancillary.RestApi.V1.SeatMaps.Requests
{
    public sealed record HoldSeatSelectionsRequest(
        string IdempotencyKey,
        long OrderId,
        int OrderCommercialVersion,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        long PointOfSaleId,
        int CurrencyId,
        DateTimeOffset? TicketedAtUtc,
        IReadOnlyList<SeatHoldSelectionInput> Seats,
        long? HoldId);
}
