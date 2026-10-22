using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed record VerifiedGroupedSelection(
        long OrderServiceId,
        string SelectionId,
        AncillaryProfile Profile,
        long ServiceDefinitionId,
        long ProvisionId,
        long? TravellerId,
        ServiceCoverageScope CoverageScope,
        IReadOnlyList<long> CoveredFlightIds,
        int Quantity);
}
