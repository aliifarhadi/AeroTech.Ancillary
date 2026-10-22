using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedMealOfferDto(
        string OfferItemRef,
        string VariantCode,
        string ServiceDefinitionRef,
        string Name,
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<string> FlightRefs,
        IReadOnlyList<string> PortionRefs,
        EnumValueDto CoverageScope,
        EnumValueDto Readiness,
        BackofficeGroupedPriceDto Price,
        string? SelectionId,
        bool RequiresDetails,
        string? DetailsEndpoint,
        EnumValueDto MealKind,
        string? MealCode,
        string? MenuItemRef,
        int CateringLeadTimeMinutes)
        : BackofficeGroupedOfferDto(
            OfferItemRef,
            VariantCode,
            ServiceDefinitionRef,
            Name,
            TravellerRefs,
            FlightRefs,
            PortionRefs,
            CoverageScope,
            Readiness,
            Price,
            SelectionId,
            RequiresDetails,
            DetailsEndpoint);
}
