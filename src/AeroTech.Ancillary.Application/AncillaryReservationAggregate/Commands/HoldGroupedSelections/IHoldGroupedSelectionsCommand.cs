using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public interface IHoldGroupedSelectionsCommand
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

        IReadOnlyList<GroupedHoldSelectionInput> Baggages { get; }

        IReadOnlyList<GroupedHoldSelectionInput> Meals { get; }

        IReadOnlyList<GroupedHoldSelectionInput> Pets { get; }

        IReadOnlyList<GroupedHoldSelectionInput> AssistedTravel { get; }

        IReadOnlyList<GroupedHoldSelectionInput> AirportServices { get; }

        IReadOnlyList<GroupedHoldSelectionInput> PriorityServices { get; }

        IReadOnlyList<GroupedHoldSelectionInput> Connectivity { get; }

        IReadOnlyList<GroupedHoldSelectionInput> Upgrades { get; }

        IReadOnlyList<GroupedHoldSelectionInput> ExtraSeats { get; }
    }
}
