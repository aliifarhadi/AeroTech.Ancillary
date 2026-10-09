using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto
{
    public sealed record BackofficeAirportSlotInventoryDto(
        long Id,
        int OwnerAirlineId,
        int AirportId,
        long FacilityId,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        int CapacityPersons,
        bool ClosedForSale,
        EnumValueDto Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<BackofficeAirportSlotAdjustmentDto> Adjustments);

    public sealed record BackofficeAirportSlotAdjustmentDto(
        long Id,
        int PreviousTotal,
        int NewTotal,
        string ReasonCode,
        long ActorId,
        string CorrelationId,
        DateTimeOffset OccurredAt,
        long ExpectedVersion,
        long ResultingVersion);
}
