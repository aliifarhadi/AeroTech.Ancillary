using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory
{
    public interface IAdjustFlightCountInventoryService
    {
        Task<FlightCountAdjustmentResult> AdjustAsync(IAdjustFlightCountInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
