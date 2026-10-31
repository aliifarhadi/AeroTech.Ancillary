namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats
{
    public interface IPriceOrderSeatsCommand : IPriceSeatsCommand
    {
        long? OrderId { get; }
    }
}
