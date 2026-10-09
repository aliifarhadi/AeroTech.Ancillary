namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory
{
    public interface IActivateAirportSlotInventoryCommand
    {
        long InventoryId { get; }

        long ExpectedVersion { get; }
    }
}
