namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory
{
    public interface IActivateFlightWeightInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
