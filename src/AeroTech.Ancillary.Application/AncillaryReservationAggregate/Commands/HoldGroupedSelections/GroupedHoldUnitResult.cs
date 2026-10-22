using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed record GroupedHoldUnitResult(long UnitId, long OrderServiceId, AncillaryReservationUnitStatus Status, string SelectionId);
}
