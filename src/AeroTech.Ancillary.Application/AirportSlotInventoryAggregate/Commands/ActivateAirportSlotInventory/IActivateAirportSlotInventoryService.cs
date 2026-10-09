using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory
{
    public interface IActivateAirportSlotInventoryService
    {
        Task<AirportSlotInventoryResult> ActivateAsync(IActivateAirportSlotInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
