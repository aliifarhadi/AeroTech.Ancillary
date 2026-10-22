namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public interface IHoldSeatSelectionsCommand
    {
        string IdempotencyKey { get; }

        long OrderId { get; }

        int OrderCommercialVersion { get; }

        string Reference { get; }

        DateTimeOffset RequestedExpiresAt { get; }

        long PointOfSaleId { get; }

        int CurrencyId { get; }

        DateTimeOffset? TicketedAtUtc { get; }

        IReadOnlyList<SeatHoldSelectionInput> Seats { get; }
    }
}
