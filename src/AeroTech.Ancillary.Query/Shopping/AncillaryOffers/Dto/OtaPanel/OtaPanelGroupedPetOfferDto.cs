namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedPetOfferDto(
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
        string TransportMode,
        decimal MaxCombinedWeightKg,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes)
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
