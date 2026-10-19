using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record AncillaryOfferDetailSnapshot(
        AncillaryShoppingSnapshot Shopping,
        AncillaryOfferItem Item,
        ServiceSpecificationArgs Specification,
        IReadOnlyList<TravellerFlightBaggageFacts> IncludedAllowance,
        OfferBaggageRule? BaggageRule,
        OfferSeatRule? SeatRule,
        OfferRestrictions Restrictions,
        OfferTerms Terms,
        IReadOnlyList<OfferPublishedRate> PublishedRates,
        IReadOnlyList<OfferOptionGroup> OptionGroups,
        OfferCommercialTerms CommercialTerms);
}
