using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory
{
    public interface ISuspendFlightCountInventoryService
    {
        Task<FlightCountInventoryResult> SuspendAsync(ISuspendFlightCountInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
