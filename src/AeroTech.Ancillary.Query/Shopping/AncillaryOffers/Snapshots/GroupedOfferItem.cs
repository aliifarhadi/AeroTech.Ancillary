using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record GroupedOfferItem(AncillaryOfferItem Item, AncillaryServiceDefinition Definition, bool RequiresDetails);
}
