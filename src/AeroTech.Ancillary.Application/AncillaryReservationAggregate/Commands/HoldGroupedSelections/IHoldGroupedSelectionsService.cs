namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public interface IHoldGroupedSelectionsService
    {
        Task<GroupedHoldResult> HoldAsync(IHoldGroupedSelectionsCommand command, CancellationToken cancellationToken = default);
    }
}
