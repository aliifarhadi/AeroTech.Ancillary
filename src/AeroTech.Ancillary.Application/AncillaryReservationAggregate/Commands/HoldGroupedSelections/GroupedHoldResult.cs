using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed record GroupedHoldResult(
        long HoldId,
        string IdempotencyKey,
        long OrderId,
        string Reference,
        AncillaryReservationStatus Status,
        DateTimeOffset? ExpiresAt,
        IReadOnlyList<GroupedHoldSeatResult> Seats,
        IReadOnlyList<GroupedHoldUnitResult> Baggages,
        IReadOnlyList<GroupedHoldUnitResult> Meals,
        IReadOnlyList<GroupedHoldUnitResult> Pets,
        IReadOnlyList<GroupedHoldUnitResult> AssistedTravel,
        IReadOnlyList<GroupedHoldUnitResult> AirportServices,
        IReadOnlyList<GroupedHoldUnitResult> PriorityServices,
        IReadOnlyList<GroupedHoldUnitResult> Connectivity,
        IReadOnlyList<GroupedHoldUnitResult> Upgrades,
        IReadOnlyList<GroupedHoldUnitResult> ExtraSeats);
}
