using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto
{
    public sealed record BackofficeFlightCountInventoryDto(
        long Id,
        int OwnerAirlineId,
        long FlightId,
        long ResourceId,
        EnumValueDto CountUnit,
        int TotalCapacity,
        bool ClosedForSale,
        EnumValueDto Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<BackofficeFlightCountAdjustmentDto> Adjustments);

    public sealed record BackofficeFlightCountAdjustmentDto(
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
