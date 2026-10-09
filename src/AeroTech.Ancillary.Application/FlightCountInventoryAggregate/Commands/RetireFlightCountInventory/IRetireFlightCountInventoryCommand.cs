namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory
{
    public interface IRetireFlightCountInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
