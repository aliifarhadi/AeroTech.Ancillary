namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory
{
    public interface IRetireAirportSlotInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
