using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections.Service
{
    public sealed record ServiceHoldSeatSelectionsCommand(
        string IdempotencyKey,
        long OrderId,
        int OrderCommercialVersion,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        long PointOfSaleId,
        int CurrencyId,
        DateTimeOffset? TicketedAtUtc,
        IReadOnlyList<SeatHoldSelectionInput> Seats,
        long? HoldId) : IRequest<AncillaryHoldResult>, IHoldSeatSelectionsCommand;
}
