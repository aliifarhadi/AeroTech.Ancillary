using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto
{
    public sealed record BackofficeFlightWeightInventoryDto(
        long Id,
        int OwnerAirlineId,
        long FlightId,
        long WeightResourceId,
        decimal CapacityKg,
        bool ClosedForSale,
        EnumValueDto Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        IReadOnlyList<BackofficeFlightWeightAdjustmentDto> Adjustments);

    public sealed record BackofficeFlightWeightAdjustmentDto(
        long Id,
        decimal PreviousKg,
        decimal NewKg,
        string ReasonCode,
        long ActorId,
        string CorrelationId,
        DateTimeOffset OccurredAt,
        long ExpectedVersion,
        long ResultingVersion);
}
