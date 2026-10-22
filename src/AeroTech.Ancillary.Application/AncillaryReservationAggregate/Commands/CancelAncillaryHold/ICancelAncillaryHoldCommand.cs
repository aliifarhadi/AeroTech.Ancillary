namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold
{
    public interface ICancelAncillaryHoldCommand
    {
        long HoldId { get; }

        string ReasonCode { get; }
    }
}
