using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory.Backoffice
{
    public sealed record BackofficeRetireAirportSlotInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<AirportSlotInventoryResult>, IRetireAirportSlotInventoryCommand;
}
