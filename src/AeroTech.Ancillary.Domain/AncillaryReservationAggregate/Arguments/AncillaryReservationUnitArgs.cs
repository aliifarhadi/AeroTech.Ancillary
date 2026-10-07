using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments
{
    public sealed record AncillaryReservationUnitArgs(
        long OrderServiceId,
        long ServiceDefinitionId,
        long ProvisionId,
        long? TravellerId,
        ServiceCoverageScope CoverageScope,
        IReadOnlyList<long> CoveredFlightIds,
        int Quantity);
}
