namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory
{
    public interface ISuspendAirportSlotInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
