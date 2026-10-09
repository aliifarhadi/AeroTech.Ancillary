using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale
{
    public interface IOpenFlightWeightInventoryForSaleService
    {
        Task<FlightWeightInventoryResult> OpenAsync(IOpenFlightWeightInventoryForSaleCommand command, CancellationToken cancellationToken = default);
    }
}
