using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts
{
    public sealed record FlightWeightInventoryReadModelSnapshot(
        long InventoryId,
        int OwnerAirlineId,
        long FlightId,
        long WeightResourceId,
        decimal CapacityKg,
        bool ClosedForSale,
        InventoryRecordStatus Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<FlightWeightAdjustmentReadModelSnapshot> Adjustments);

    public sealed record FlightWeightAdjustmentReadModelSnapshot(
        long AdjustmentId,
        decimal PreviousKg,
        decimal NewKg,
        string ReasonCode,
        long ActorId,
        string CorrelationId,
        DateTimeOffset OccurredAt,
        long ExpectedVersion,
        long ResultingVersion);
}
