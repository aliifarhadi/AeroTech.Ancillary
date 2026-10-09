using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory
{
    public interface ISuspendAirportSlotInventoryService
    {
        Task<AirportSlotInventoryResult> SuspendAsync(ISuspendAirportSlotInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
