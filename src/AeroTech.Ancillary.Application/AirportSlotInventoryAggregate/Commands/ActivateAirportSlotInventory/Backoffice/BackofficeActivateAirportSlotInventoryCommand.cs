using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory.Backoffice
{
    public sealed record BackofficeActivateAirportSlotInventoryCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<AirportSlotInventoryResult>, IActivateAirportSlotInventoryCommand;
}
