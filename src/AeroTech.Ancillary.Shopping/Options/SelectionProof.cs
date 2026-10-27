using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Options
{
    public sealed record SelectionProof(
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        string SourceBinding,
        string SalesBinding,
        int Quantity,
        DateTimeOffset ExpiresAtUtc,
        AncillaryServiceDefinition Definition);
}
