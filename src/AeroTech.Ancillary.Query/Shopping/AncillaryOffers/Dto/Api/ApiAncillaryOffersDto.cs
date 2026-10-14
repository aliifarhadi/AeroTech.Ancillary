using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiAncillaryOffersDto(
        ApiMetaDto Meta,
        ApiAncillaryOffersDataDto Data,
        ApiIncludedDto Included);

    public sealed record ApiMetaDto(int Count, int WithheldCount, string Status);

    public sealed record ApiAncillaryOffersDataDto(
        string Type,
        string FlightOfferId,
        IReadOnlyList<ApiTravelerDto> Travelers,
        IReadOnlyList<ApiSegmentDto> Segments);

    public sealed record ApiTravelerDto(string Id, string TravelerType);

    public sealed record ApiSegmentDto(
        string Id,
        string? CarrierCode,
        string? Number,
        ApiEndpointDto Departure,
        ApiEndpointDto Arrival,
        ApiOperatingDto Operating);

    public sealed record ApiEndpointDto(string? IataCode, DateTimeOffset At);

    public sealed record ApiOperatingDto(string? CarrierCode);

    public sealed record ApiIncludedDto(
        IReadOnlyDictionary<string, ApiBagDto> Bags,
        [property: JsonPropertyName("other-services")] IReadOnlyDictionary<string, ApiOtherServiceDto> OtherServices);

    public sealed record ApiBagDto(
        int? Quantity,
        decimal? Weight,
        string? WeightUnit,
        string Name,
        ApiElementaryPriceDto? Price,
        bool BookableByItinerary,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> TravelerIds,
        ApiAncillaryDto Ancillary);

    public sealed record ApiOtherServiceDto(
        string Name,
        ApiElementaryPriceDto? Price,
        bool BookableByTraveler,
        bool BookableByItinerary,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> TravelerIds,
        ApiAncillaryDto Ancillary);

    public sealed record ApiElementaryPriceDto(string Amount, string? CurrencyCode);

    public sealed record ApiAncillaryDto(
        string Type,
        ApiServiceDto Service,
        ApiPricingDto Pricing,
        ApiQuantityDto Quantity,
        IReadOnlyList<ApiSelectionRequirementDto> SelectionRequirements,
        ApiAvailabilityDto Availability,
        string ConfirmationRequirement,
        string Readiness,
        bool CanPurchase,
        IReadOnlyList<string> Reasons);

    public sealed record ApiServiceDto(
        string ServiceDefinitionRef,
        string CommercialName,
        string? Description,
        string Category,
        string VariantCode,
        string ServiceTypeCode,
        string ServiceSubCode,
        string? Rfic,
        string? Rfisc,
        string DocumentType,
        string BookingMethod,
        string? SsrCode);

    public sealed record ApiPricingDto(
        string Origin,
        string Status,
        string? Unit,
        string? CurrencyCode,
        string? Base,
        string? Total,
        IReadOnlyList<ApiTaxDto> Taxes,
        IReadOnlyList<ApiFeeDto> Fees,
        string? RequestedTotal,
        bool TotalComplete);

    public sealed record ApiTaxDto(string Amount, string? Code, string? CurrencyCode, bool Included);

    public sealed record ApiFeeDto(string Amount, string? Type, string? CurrencyCode, string? AppliesPer, bool OrderLevel);

    public sealed record ApiQuantityDto(string Unit, int Min, int Max);

    public sealed record ApiSelectionRequirementDto(string Name, string Type, bool Required);

    public sealed record ApiAvailabilityDto(string State, bool Guaranteed, bool RequiresCheckAtReserve);

    public sealed record ApiAncillaryOfferDetailDto(ApiAncillaryOfferDetailDataDto Data);

    public sealed record ApiAncillaryOfferDetailDataDto(
        string Type,
        string Id,
        string FlightOfferId,
        ApiBagDto? Bag,
        ApiOtherServiceDto? OtherService,
        ApiServiceSpecificationDto Specification,
        IReadOnlyList<ApiIncludedAllowanceDto> IncludedAllowance,
        ApiBaggageRuleDto? BaggageRule,
        ApiSeatRuleDto? SeatRule,
        ApiRestrictionsDto Restrictions,
        ApiTermsDto Terms,
        IReadOnlyList<ApiRateDto> Rates);

    public sealed record ApiSelectionResultDto(ApiSelectionResultDataDto Data);

    public sealed record ApiSelectionResultDataDto(
        string Type,
        string FlightOfferId,
        string Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool OfferItemChanged,
        ApiBagDto? Bag,
        ApiOtherServiceDto? OtherService,
        ApiElementaryPriceDto? SelectedPrice,
        IReadOnlyList<ApiSelectionIssueDto> Issues,
        bool CanPurchase);

    public sealed record ApiSelectionIssueDto(string Field, string Reason);
}
