using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests
{
    public sealed record HoldGroupedSelectionsRequest(
        string IdempotencyKey,
        long OrderId,
        int OrderCommercialVersion,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        long PointOfSaleId,
        int CurrencyId,
        DateTimeOffset? TicketedAtUtc,
        IReadOnlyList<SeatHoldSelectionInput>? Seats,
        IReadOnlyList<GroupedHoldSelectionInput>? Baggages,
        IReadOnlyList<GroupedHoldSelectionInput>? Meals,
        IReadOnlyList<GroupedHoldSelectionInput>? Pets,
        IReadOnlyList<GroupedHoldSelectionInput>? AssistedTravel,
        IReadOnlyList<GroupedHoldSelectionInput>? AirportServices,
        IReadOnlyList<GroupedHoldSelectionInput>? PriorityServices,
        IReadOnlyList<GroupedHoldSelectionInput>? Connectivity,
        IReadOnlyList<GroupedHoldSelectionInput>? Upgrades,
        IReadOnlyList<GroupedHoldSelectionInput>? ExtraSeats);
}
