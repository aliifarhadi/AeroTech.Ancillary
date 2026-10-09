using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory
{
    public sealed record FlightWeightInventoryResult(
        long Id,
        int OwnerAirlineId,
        long FlightId,
        long WeightResourceId,
        decimal CapacityKg,
        bool ClosedForSale,
        InventoryRecordStatus Status,
        long Version);

    public sealed record FlightWeightAdjustmentResult(
        long InventoryId,
        long AdjustmentId,
        decimal PreviousKg,
        decimal NewKg,
        long Version);
}
