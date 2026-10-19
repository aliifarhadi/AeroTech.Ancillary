using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeSelectionResultDto(
        string OfferId,
        EnumValueDto Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool ProvisionChangedBySelection,
        BackofficeAncillaryOfferDto Offer,
        decimal? SelectedQuantityTotal,
        IReadOnlyList<BackofficeFieldIssueDto> FieldIssues,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        IReadOnlyList<OfferSelectedOptionDto> SelectedOptions,
        OfferBeneficiaryDto Beneficiary,
        OfferSelectionCoverageDto Coverage,
        OfferExactPriceDto ExactPrice,
        bool RequiresCheckAtReserve);
}
