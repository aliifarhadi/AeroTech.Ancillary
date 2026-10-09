using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory.Backoffice
{
    public sealed record BackofficeSuspendFlightCountInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<FlightCountInventoryResult>, ISuspendFlightCountInventoryCommand;
}
