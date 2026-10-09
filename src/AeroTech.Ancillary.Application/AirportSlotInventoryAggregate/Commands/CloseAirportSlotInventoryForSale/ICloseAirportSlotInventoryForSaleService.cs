using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale
{
    public interface ICloseAirportSlotInventoryForSaleService
    {
        Task<AirportSlotInventoryResult> CloseAsync(ICloseAirportSlotInventoryForSaleCommand command, CancellationToken cancellationToken = default);
    }
}
