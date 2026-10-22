using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed record SeatSelectionVerification(
        long OrderId,
        int OrderCommercialVersion,
        long PointOfSaleId,
        int CurrencyId,
        DateTimeOffset? TicketedAtUtc,
        IReadOnlyList<SeatHoldSelectionInput> Seats);
}
