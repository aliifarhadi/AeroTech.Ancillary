using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelAncillaryOfferDetailDto(
        string OfferId,
        string TravellerRef,
        OtaPanelServiceDto Service,
        OtaPanelPriceBreakdownDto PriceBreakdown,
        OtaPanelServiceSpecificationDto Specification,
        IReadOnlyList<OtaPanelIncludedAllowanceDto> IncludedAllowance,
        OtaPanelBaggageRuleDto? BaggageRule,
        OtaPanelSeatRuleDto? SeatRule,
        OtaPanelRestrictionsDto Restrictions,
        OtaPanelTermsDto Terms,
        IReadOnlyList<OtaPanelRateDto> PriceRows,
        OfferSelectionContractDto SelectionContract,
        OfferDetailQuantityDto Quantity,
        IReadOnlyList<OfferOptionGroupDto> OptionGroups,
        string OptionSourceState,
        OfferPriceCompletenessDto PriceCompleteness,
        OfferCommercialTermsDto CommercialTerms);
}
