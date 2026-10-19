using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelSelectionResultDto(
        string OfferId,
        string Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool ServiceChanged,
        OtaPanelServiceDto Service,
        OtaPanelMoneyDto? SelectedAmount,
        IReadOnlyList<OtaPanelInputIssueDto> InputIssues,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        IReadOnlyList<OfferSelectedOptionDto> SelectedOptions,
        OfferBeneficiaryDto Beneficiary,
        OfferSelectionCoverageDto Coverage,
        OfferExactPriceDto ExactPrice,
        bool RequiresCheckAtReserve);
}
