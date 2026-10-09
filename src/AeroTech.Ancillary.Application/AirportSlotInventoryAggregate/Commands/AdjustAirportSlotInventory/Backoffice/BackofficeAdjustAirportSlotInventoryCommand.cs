using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory.Backoffice
{
    public sealed record BackofficeAdjustAirportSlotInventoryCommand(
        long InventoryId,
        int NewTotal,
        string ReasonCode,
        string CorrelationId,
        long ExpectedVersion) : IRequest<AirportSlotAdjustmentResult>, IAdjustAirportSlotInventoryCommand;
}
