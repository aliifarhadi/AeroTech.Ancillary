using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedConnectivityOfferDto(
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
        BackofficeGroupedQuantityDto? Quantity,
        string? SelectionId,
        bool RequiresDetails,
        string? DetailsEndpoint,
        EnumValueDto PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices)
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
            Quantity,
            SelectionId,
            RequiresDetails,
            DetailsEndpoint);
}
