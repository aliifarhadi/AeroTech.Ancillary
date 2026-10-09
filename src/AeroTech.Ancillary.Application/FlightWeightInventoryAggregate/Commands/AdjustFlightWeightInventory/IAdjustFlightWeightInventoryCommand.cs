namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory
{
    public interface IAdjustFlightWeightInventoryCommand
    {
        long InventoryId { get; }

        decimal NewKg { get; }

        string ReasonCode { get; }

        string CorrelationId { get; }

        long ExpectedVersion { get; }
    }
}
