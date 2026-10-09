using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory
{
    public interface ISuspendFlightWeightInventoryService
    {
        Task<FlightWeightInventoryResult> SuspendAsync(ISuspendFlightWeightInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
