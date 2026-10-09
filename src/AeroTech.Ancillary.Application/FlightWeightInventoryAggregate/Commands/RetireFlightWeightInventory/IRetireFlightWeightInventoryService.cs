using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory
{
    public interface IRetireFlightWeightInventoryService
    {
        Task<FlightWeightInventoryResult> RetireAsync(IRetireFlightWeightInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
