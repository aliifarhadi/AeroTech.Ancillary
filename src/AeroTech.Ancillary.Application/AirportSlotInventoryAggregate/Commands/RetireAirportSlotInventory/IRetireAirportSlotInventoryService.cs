using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;

namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory
{
    public interface IRetireAirportSlotInventoryService
    {
        Task<AirportSlotInventoryResult> RetireAsync(IRetireAirportSlotInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
