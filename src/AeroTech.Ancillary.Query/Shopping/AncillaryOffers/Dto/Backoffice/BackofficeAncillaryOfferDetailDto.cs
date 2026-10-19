using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryOfferDetailDto(
        string OfferId,
        BackofficeAncillaryOfferDto Offer,
        ServiceSpecificationDto Specification,
        IReadOnlyList<BackofficeIncludedAllowanceDto> IncludedAllowance,
        BackofficeBaggageRuleDto? BaggageRule,
        BackofficeSeatRuleDto? SeatRule,
        BackofficeRestrictionsDto Restrictions,
        BackofficeTermsDto Terms,
        IReadOnlyList<BackofficeRateDto> PublishedRates,
        OfferSelectionContractDto SelectionContract,
        OfferDetailQuantityDto Quantity,
        IReadOnlyList<OfferOptionGroupDto> OptionGroups,
        string OptionSourceState,
        OfferPriceCompletenessDto PriceCompleteness,
        OfferCommercialTermsDto CommercialTerms);
}
