using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed record AncillaryHoldServiceInput(
        long OrderServiceId,
        long ServiceDefinitionId,
        long ProvisionId,
        long? TravellerId,
        ServiceCoverageScope CoverageScope,
        IReadOnlyList<long> CoveredFlightIds,
        int Quantity);
}
