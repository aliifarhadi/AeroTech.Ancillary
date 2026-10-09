using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory.Backoffice
{
    public sealed record BackofficeDefineFlightCountInventoryCommand(
        int OwnerAirlineId,
        long FlightId,
        long ResourceId,
        InventoryCountUnit CountUnit,
        int TotalCapacity) : IRequest<FlightCountInventoryResult>, IDefineFlightCountInventoryCommand;
}
