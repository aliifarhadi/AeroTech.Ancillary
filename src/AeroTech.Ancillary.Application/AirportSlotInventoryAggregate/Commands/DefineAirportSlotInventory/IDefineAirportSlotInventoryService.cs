namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory
{
    public interface IDefineAirportSlotInventoryService
    {
        Task<AirportSlotInventoryResult> DefineAsync(IDefineAirportSlotInventoryCommand command, CancellationToken cancellationToken = default);
    }
}
