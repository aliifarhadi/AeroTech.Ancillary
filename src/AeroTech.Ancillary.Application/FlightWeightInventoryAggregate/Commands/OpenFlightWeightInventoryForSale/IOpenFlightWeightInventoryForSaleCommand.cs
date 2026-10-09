namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale
{
    public interface IOpenFlightWeightInventoryForSaleCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
