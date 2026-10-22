using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedPetOfferDto(
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
        EnumValueDto TransportMode,
        decimal MaxCombinedWeightKg,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes)
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
