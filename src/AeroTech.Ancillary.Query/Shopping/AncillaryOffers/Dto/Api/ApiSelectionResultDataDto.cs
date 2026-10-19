using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
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
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        IReadOnlyList<OfferSelectedOptionDto> SelectedOptions,
        OfferBeneficiaryDto Beneficiary,
        OfferSelectionCoverageDto Coverage,
        OfferExactPriceDto ExactPrice,
        bool RequiresCheckAtReserve);
}
