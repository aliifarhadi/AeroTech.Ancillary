using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedServiceDto(
        string Kind,
        string ServiceDefinitionRef,
        long DefinitionVersionId,
        long ProvisionId,
        long? PricingRevisionId,
        string? Rfic,
        string? Rfisc,
        string? SsrCode,
        string CoverageScope,
        IReadOnlyList<long> OrderTravellerIds,
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<long> FlightIds,
        IReadOnlyList<string> PortionRefs,
        string? SeatNumber,
        long? FlightCapacityId,
        IReadOnlyList<OfferSelectedOptionDto> SelectedOptions,
        OfferExactPriceDto Price,
        bool RequiresCheckAtReserve,
        string VariantCode,
        string ServiceTypeCode,
        string ServiceSubCode,
        string BookingMethod,
        bool BookingRequired,
        string DocumentRouting,
        string DocumentType,
        bool DocumentRequired,
        string ConfirmationRequirement,
        string? FulfillmentProviderKey,
        long? PricingRateId,
        ServiceAncillarySelectionDto Selection);
}
