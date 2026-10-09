using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale.Backoffice
{
    public sealed record BackofficeCloseAirportSlotInventoryForSaleCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<AirportSlotInventoryResult>, ICloseAirportSlotInventoryForSaleCommand;
}
