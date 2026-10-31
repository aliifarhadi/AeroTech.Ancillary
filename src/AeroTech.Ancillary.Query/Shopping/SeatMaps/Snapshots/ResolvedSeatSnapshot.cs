using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots
{
    public sealed record ResolvedSeatSnapshot(
        string Id,
        SeatMapSelection Selection,
        AncillaryServiceDefinition Definition,
        AncillaryProvision Provision,
        AncillaryPricing Pricing,
        AncillaryPricingRate Rate);
}
