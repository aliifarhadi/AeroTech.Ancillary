using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory
{
    public interface IAdjustFlightWeightInventoryService
    {
        Task<FlightWeightAdjustmentResult> AdjustAsync(IAdjustFlightWeightInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
