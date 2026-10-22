using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed record GroupedHoldSeatResult(long UnitId, long OrderServiceId, AncillaryReservationUnitStatus Status, string SeatMapSelectionId, string SeatNumber);
}
