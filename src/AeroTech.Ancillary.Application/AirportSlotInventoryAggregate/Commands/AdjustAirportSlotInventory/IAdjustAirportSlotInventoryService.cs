using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory
{
    public interface IAdjustAirportSlotInventoryService
    {
        Task<AirportSlotAdjustmentResult> AdjustAsync(IAdjustAirportSlotInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
