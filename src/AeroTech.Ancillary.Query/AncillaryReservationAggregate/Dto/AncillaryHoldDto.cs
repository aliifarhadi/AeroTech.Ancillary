using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record AncillaryHoldDto(
        long HoldId,
        string IdempotencyKey,
        long OrderId,
        string Reference,
        AncillaryReservationStatus Status,
        DateTimeOffset? ExpiresAt,
        IReadOnlyList<AncillaryHoldUnitDto> Units);

    public sealed record AncillaryHoldUnitDto(
        long UnitId,
        long OrderServiceId,
        long ServiceDefinitionId,
        long ProvisionId,
        long? TravellerId,
        ServiceCoverageScope CoverageScope,
        IReadOnlyList<long> CoveredFlightIds,
        int Quantity,
        AncillaryReservationUnitStatus Status);
}
