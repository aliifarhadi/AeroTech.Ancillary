namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedMealOfferDto(
        string OfferItemRef,
        string VariantCode,
        string ServiceDefinitionRef,
        string Name,
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<string> FlightRefs,
        IReadOnlyList<string> PortionRefs,
        string CoverageScope,
        string Readiness,
        OtaPanelGroupedPriceDto Price,
        string? SelectionId,
        bool RequiresDetails,
        string? DetailsEndpoint,
        string MealKind,
        string? MealCode,
        string? MenuItemRef,
        int CateringLeadTimeMinutes)
        : OtaPanelGroupedOfferDto(
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
