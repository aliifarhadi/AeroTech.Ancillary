namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale
{
    public interface ICloseFlightWeightInventoryForSaleCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
