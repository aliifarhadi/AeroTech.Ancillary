namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory
{
    public interface IRetireFlightWeightInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
