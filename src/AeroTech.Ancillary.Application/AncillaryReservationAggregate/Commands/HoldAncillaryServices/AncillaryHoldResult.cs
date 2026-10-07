using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed record AncillaryHoldResult(
        long HoldId,
        string IdempotencyKey,
        long OrderId,
        string Reference,
        AncillaryReservationStatus Status,
        DateTimeOffset? ExpiresAt,
        IReadOnlyList<AncillaryHoldUnitResult> Units);

    public sealed record AncillaryHoldUnitResult(
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
