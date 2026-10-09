using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory.Backoffice
{
    public sealed record BackofficeRetireFlightCountInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightCountInventoryResult>, IRetireFlightCountInventoryCommand;
}
