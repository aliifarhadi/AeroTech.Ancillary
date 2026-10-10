using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record CandidateIdentity(
        long DefinitionVersionId,
        long ProvisionId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        long? PricingRevisionId);
}
