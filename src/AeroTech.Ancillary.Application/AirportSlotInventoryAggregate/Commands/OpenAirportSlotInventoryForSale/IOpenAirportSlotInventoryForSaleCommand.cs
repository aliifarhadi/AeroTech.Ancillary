namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale
{
    public interface IOpenAirportSlotInventoryForSaleCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
