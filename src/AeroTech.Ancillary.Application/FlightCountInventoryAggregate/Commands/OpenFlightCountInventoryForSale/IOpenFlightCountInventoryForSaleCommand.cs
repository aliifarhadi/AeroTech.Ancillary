namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale
{
    public interface IOpenFlightCountInventoryForSaleCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
