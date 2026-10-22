namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedConnectivityOfferDto(
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
        string PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices)
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
