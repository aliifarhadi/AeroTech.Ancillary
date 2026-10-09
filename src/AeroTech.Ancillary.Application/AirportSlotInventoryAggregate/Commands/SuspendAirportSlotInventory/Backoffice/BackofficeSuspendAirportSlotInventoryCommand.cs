using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory.Backoffice
{
    public sealed record BackofficeSuspendAirportSlotInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<AirportSlotInventoryResult>, ISuspendAirportSlotInventoryCommand;
}
