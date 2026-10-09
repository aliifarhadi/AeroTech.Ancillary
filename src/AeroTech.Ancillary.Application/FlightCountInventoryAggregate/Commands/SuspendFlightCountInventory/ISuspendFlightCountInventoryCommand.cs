namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory
{
    public interface ISuspendFlightCountInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
