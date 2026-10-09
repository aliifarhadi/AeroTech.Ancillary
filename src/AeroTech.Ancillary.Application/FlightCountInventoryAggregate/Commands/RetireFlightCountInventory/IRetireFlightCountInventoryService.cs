using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory
{
    public interface IRetireFlightCountInventoryService
    {
        Task<FlightCountInventoryResult> RetireAsync(IRetireFlightCountInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
