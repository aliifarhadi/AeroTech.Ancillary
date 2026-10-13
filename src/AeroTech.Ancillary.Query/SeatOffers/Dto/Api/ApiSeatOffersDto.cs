using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;

namespace AeroTech.Ancillary.Query.SeatOffers.Dto.Api
{
    public sealed record ApiSeatOffersDto(ApiMetaDto Meta, ApiSeatOffersDataDto Data, ApiSeatIncludedDto Included);

    public sealed record ApiSeatOffersDataDto(
        string Type,
        string FlightOfferId,
        IReadOnlyList<ApiTravelerDto> Travelers,
        IReadOnlyList<ApiSegmentDto> Segments);

    public sealed record ApiSeatIncludedDto([property: JsonPropertyName("seat-offers")] IReadOnlyDictionary<string, ApiSeatOfferDto> SeatOffers);

    public sealed record ApiSeatOfferDto(
        string Name,
        ApiElementaryPriceDto? Price,
        bool BookableByTraveler,
        bool BookableByItinerary,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> TravelerIds,
        ApiSeatTermsDto Seat,
        ApiSeatAncillaryDto Ancillary);

    public sealed record ApiSeatTermsDto(
        string Purpose,
        IReadOnlyList<string> CharacteristicCodes,
        IReadOnlyList<int> CabinIds,
        bool ExitRowEligibilityRequired,
        bool AdjacentSeatRequired,
        string? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        IReadOnlyList<string> SeatNumbers);

    public sealed record ApiSeatAncillaryDto(
        string Type,
        ApiServiceDto Service,
        ApiPricingDto Pricing,
        ApiQuantityDto Quantity,
        IReadOnlyList<ApiSelectionRequirementDto> SelectionRequirements,
        string ConfirmationRequirement,
        string Readiness,
        bool CanPurchase,
        IReadOnlyList<string> Reasons);

    public sealed record ApiSeatOfferDetailDto(ApiSeatOfferDetailDataDto Data);

    public sealed record ApiSeatOfferDetailDataDto(
        string Type,
        string Id,
        string FlightOfferId,
        ApiSeatOfferDto SeatOffer,
        ApiSeatSpecificationDto Specification,
        ApiSeatRuleDto? SeatRule,
        ApiRestrictionsDto Restrictions,
        ApiTermsDto Terms,
        IReadOnlyList<ApiRateDto> Rates);

    public sealed record ApiSeatSelectionResultDto(ApiSeatSelectionResultDataDto Data);

    public sealed record ApiSeatSelectionResultDataDto(
        string Type,
        string FlightOfferId,
        string Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool OfferItemChanged,
        ApiSeatOfferDto SeatOffer,
        ApiElementaryPriceDto? SelectedPrice,
        IReadOnlyList<ApiSelectionIssueDto> Issues,
        bool CanPurchase);
}
