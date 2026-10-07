namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold
{
    public interface IConfirmAncillaryHoldCommand
    {
        long HoldId { get; }
    }
}
