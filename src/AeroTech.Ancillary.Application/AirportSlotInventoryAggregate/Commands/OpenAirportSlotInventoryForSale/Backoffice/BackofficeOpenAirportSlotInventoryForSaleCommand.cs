using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale.Backoffice
{
    public sealed record BackofficeOpenAirportSlotInventoryForSaleCommand(
        long InventoryId,
        long ExpectedVersion) : IRequest<AirportSlotInventoryResult>, IOpenAirportSlotInventoryForSaleCommand;
}
