namespace AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory
{
    public interface IAdjustAirportSlotInventoryCommand
    {
        long InventoryId { get; }

        int NewTotal { get; }

        string ReasonCode { get; }

        string CorrelationId { get; }

        long ExpectedVersion { get; }
    }
}
