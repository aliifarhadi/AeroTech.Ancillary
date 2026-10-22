using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers
{
    public static class GroupedOfferSpecifications
    {
        public static IReadOnlyList<string> AllowedSsrCodes(AssistedTravelSpecification specification)
            => (specification.Wheelchair?.AllowedSsrCodes ?? specification.DisabilityAssistance?.AllowedSsrCodes ?? [])
                .Select(code => code.Code)
                .Order(StringComparer.Ordinal)
                .ToList();
    }
}
