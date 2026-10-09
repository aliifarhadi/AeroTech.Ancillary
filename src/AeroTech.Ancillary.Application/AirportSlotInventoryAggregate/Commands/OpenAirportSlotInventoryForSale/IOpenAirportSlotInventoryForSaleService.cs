using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale
{
    public interface IOpenAirportSlotInventoryForSaleService
    {
        Task<AirportSlotInventoryResult> OpenAsync(IOpenAirportSlotInventoryForSaleCommand command, CancellationToken cancellationToken = default);
    }
}
