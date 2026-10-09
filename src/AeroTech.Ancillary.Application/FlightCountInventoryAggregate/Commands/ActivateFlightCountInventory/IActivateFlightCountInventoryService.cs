using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory
{
    public interface IActivateFlightCountInventoryService
    {
        Task<FlightCountInventoryResult> ActivateAsync(IActivateFlightCountInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
