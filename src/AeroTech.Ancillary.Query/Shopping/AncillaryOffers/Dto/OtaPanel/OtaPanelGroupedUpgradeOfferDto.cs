namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedUpgradeOfferDto(
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
        OtaPanelGroupedQuantityDto? Quantity,
        string? SelectionId,
        bool RequiresDetails,
        string? DetailsEndpoint,
        int FromCabinId,
        int ToCabinId,
        string AllowedUpgradeKind,
        bool RequiresTicketExchange)
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
            Quantity,
            SelectionId,
            RequiresDetails,
            DetailsEndpoint);
}
