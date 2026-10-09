using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts
{
    public sealed record FlightCountInventoryReadModelSnapshot(
        long InventoryId,
        int OwnerAirlineId,
        long FlightId,
        long ResourceId,
        InventoryCountUnit CountUnit,
        int TotalCapacity,
        bool ClosedForSale,
        InventoryRecordStatus Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<FlightCountAdjustmentReadModelSnapshot> Adjustments);

    public sealed record FlightCountAdjustmentReadModelSnapshot(
        long AdjustmentId,
        int PreviousTotal,
        int NewTotal,
        string ReasonCode,
        long ActorId,
        string CorrelationId,
        DateTimeOffset OccurredAt,
        long ExpectedVersion,
        long ResultingVersion);
}
