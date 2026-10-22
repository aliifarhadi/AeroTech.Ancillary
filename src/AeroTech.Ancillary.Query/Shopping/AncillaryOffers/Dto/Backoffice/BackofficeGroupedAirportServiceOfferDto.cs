using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedAirportServiceOfferDto(
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
        EnumValueDto Kind,
        int AirportId,
        long? FacilityId,
        bool RequiresSpecificAppointment,
        int? MaxGuestsPerPrimary)
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
