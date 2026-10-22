using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections.Service
{
    public sealed record ServiceHoldGroupedSelectionsCommand(
        string IdempotencyKey,
        long OrderId,
        int OrderCommercialVersion,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        long PointOfSaleId,
        int CurrencyId,
        DateTimeOffset? TicketedAtUtc,
        IReadOnlyList<SeatHoldSelectionInput> Seats,
        IReadOnlyList<GroupedHoldSelectionInput> Baggages,
        IReadOnlyList<GroupedHoldSelectionInput> Meals,
        IReadOnlyList<GroupedHoldSelectionInput> Pets,
        IReadOnlyList<GroupedHoldSelectionInput> AssistedTravel,
        IReadOnlyList<GroupedHoldSelectionInput> AirportServices,
        IReadOnlyList<GroupedHoldSelectionInput> PriorityServices,
        IReadOnlyList<GroupedHoldSelectionInput> Connectivity,
        IReadOnlyList<GroupedHoldSelectionInput> Upgrades,
        IReadOnlyList<GroupedHoldSelectionInput> ExtraSeats) : IRequest<GroupedHoldResult>, IHoldGroupedSelectionsCommand;
}
