using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query._Shared.Enums;

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
        IReadOnlyList<BackofficeRateDto> PublishedRates);

    public sealed record BackofficeSelectionResultDto(
        string OfferId,
        EnumValueDto Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool ProvisionChangedBySelection,
        BackofficeAncillaryOfferDto Offer,
        decimal? SelectedQuantityTotal,
        IReadOnlyList<BackofficeFieldIssueDto> FieldIssues,
        bool CanPurchase);

    public sealed record BackofficeFieldIssueDto(string Field, string ReasonCode);
}
