using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory.Backoffice
{
    public sealed record BackofficeActivateFlightCountInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightCountInventoryResult>, IActivateFlightCountInventoryCommand;
}
