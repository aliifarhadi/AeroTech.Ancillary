using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale
{
    public interface ICloseFlightCountInventoryForSaleService
    {
        Task<FlightCountInventoryResult> CloseAsync(ICloseFlightCountInventoryForSaleCommand command, CancellationToken cancellationToken = default);
    }
}
