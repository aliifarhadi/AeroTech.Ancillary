namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold
{
    public interface IReleaseAncillaryHoldCommand
    {
        long HoldId { get; }
    }
}
