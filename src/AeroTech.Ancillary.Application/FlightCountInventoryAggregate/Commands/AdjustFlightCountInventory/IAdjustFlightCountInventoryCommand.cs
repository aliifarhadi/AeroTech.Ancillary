namespace AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory
{
    public interface IAdjustFlightCountInventoryCommand
    {
        long InventoryId { get; }

        int NewTotal { get; }

        string ReasonCode { get; }

        string CorrelationId { get; }

        long ExpectedVersion { get; }
    }
}
