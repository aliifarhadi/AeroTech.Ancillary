using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory
{
    public interface IActivateFlightWeightInventoryService
    {
        Task<FlightWeightInventoryResult> ActivateAsync(IActivateFlightWeightInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
