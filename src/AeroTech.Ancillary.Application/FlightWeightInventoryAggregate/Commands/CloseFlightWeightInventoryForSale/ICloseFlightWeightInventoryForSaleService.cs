using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale
{
    public interface ICloseFlightWeightInventoryForSaleService
    {
        Task<FlightWeightInventoryResult> CloseAsync(ICloseFlightWeightInventoryForSaleCommand command, CancellationToken cancellationToken = default);
    }
}
