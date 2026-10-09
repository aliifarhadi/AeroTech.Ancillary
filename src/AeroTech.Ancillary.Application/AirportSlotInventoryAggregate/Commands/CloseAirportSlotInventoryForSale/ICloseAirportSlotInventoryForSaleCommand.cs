namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale
{
    public interface ICloseAirportSlotInventoryForSaleCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
