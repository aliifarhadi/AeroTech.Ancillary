namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory
{
    public interface ISuspendFlightWeightInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
