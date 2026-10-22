using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed record GroupedHoldSelectionEntry(AncillaryProfile Profile, long OrderServiceId, string SelectionId);
}
