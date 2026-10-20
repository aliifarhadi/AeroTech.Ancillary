using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel
{
    public static class OtaPanelSelectionResultMapper
    {
        public static OtaPanelSelectionResultDto Map(AncillarySelectionSnapshot selection)
            => Map(selection, OfferPurchase.CanPurchase(selection.Evaluation), null);

        public static OtaPanelSelectionResultDto Map(AncillarySelectionSnapshot selection, bool canPurchase, PricedSelectionToken? pricedSelection)
            => new(
                selection.Shopping.Offer.OfferId,
                selection.Evaluation.Status.ToString(),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                OtaPanelAncillaryOffersMapper.Service(selection.CurrentOfferItemRef, selection.Evaluation.Candidate),
                OtaPanelAncillaryOffersMapper.Money(selection.Evaluation.Candidate.Price.RequestedQuantityTotal, selection.Evaluation.Candidate.Price.CurrencyId),
                selection.Evaluation.FieldIssues.Select(issue => new OtaPanelInputIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                canPurchase,
                pricedSelection?.Value,
                pricedSelection?.ExpiresAtUtc,
                OfferSelectionResultsMapper.SelectedOptions(selection),
                OfferSelectionResultsMapper.Beneficiary(selection, refs => refs),
                OfferSelectionResultsMapper.Coverage(selection, refs => refs),
                OfferSelectionResultsMapper.ExactPrice(selection.Evaluation.Candidate.Price, canPurchase),
                selection.Evaluation.Candidate.Availability.RequiresCheckAtReserve);
    }
}
