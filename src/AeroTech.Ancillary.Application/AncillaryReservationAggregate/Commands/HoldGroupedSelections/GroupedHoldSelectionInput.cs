namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed record GroupedHoldSelectionInput(long OrderServiceId, string SelectionId);
}
