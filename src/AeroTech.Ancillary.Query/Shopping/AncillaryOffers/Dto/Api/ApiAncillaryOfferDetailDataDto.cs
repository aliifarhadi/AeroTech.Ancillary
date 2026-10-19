using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
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
        IReadOnlyList<ApiRateDto> Rates,
        OfferSelectionContractDto SelectionContract,
        OfferDetailQuantityDto Quantity,
        IReadOnlyList<OfferOptionGroupDto> OptionGroups,
        string OptionSourceState,
        OfferPriceCompletenessDto PriceCompleteness,
        OfferCommercialTermsDto CommercialTerms);
}
