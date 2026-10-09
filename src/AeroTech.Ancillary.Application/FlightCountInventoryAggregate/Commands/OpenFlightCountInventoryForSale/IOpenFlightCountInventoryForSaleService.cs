using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale
{
    public interface IOpenFlightCountInventoryForSaleService
    {
        Task<FlightCountInventoryResult> OpenAsync(IOpenFlightCountInventoryForSaleCommand command, CancellationToken cancellationToken = default);
    }
}
