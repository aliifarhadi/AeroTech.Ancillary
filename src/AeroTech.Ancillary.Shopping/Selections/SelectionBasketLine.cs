using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed record SelectionBasketLine(
        long PricingRateId,
        ServiceCoverageScope CoverageScope,
        string TravellerRef,
        string CoverageRef,
        AncillaryServiceDefinition Definition);
}
