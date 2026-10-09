using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts
{
    public sealed record AirportSlotInventoryReadModelSnapshot(
        long InventoryId,
        int OwnerAirlineId,
        int AirportId,
        long FacilityId,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        int CapacityPersons,
        bool ClosedForSale,
        InventoryRecordStatus Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<AirportSlotAdjustmentReadModelSnapshot> Adjustments);

    public sealed record AirportSlotAdjustmentReadModelSnapshot(
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
