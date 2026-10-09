namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory
{
    public interface IActivateFlightCountInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
