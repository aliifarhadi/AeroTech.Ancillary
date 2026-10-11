using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.OtaPanel
{
    public static class OtaPanelSelectionResultMapper
    {
        public static OtaPanelSelectionResultDto Map(AncillarySelectionSnapshot selection)
            => new(
                selection.Shopping.Offer.OfferId,
                selection.Evaluation.Status.ToString(),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                OtaPanelAncillaryOffersMapper.Service(selection.CurrentOfferItemRef, selection.Evaluation.Candidate),
                OtaPanelAncillaryOffersMapper.Money(selection.Evaluation.Candidate.Price.RequestedQuantityTotal, selection.Evaluation.Candidate.Price.CurrencyId),
                selection.Evaluation.FieldIssues.Select(issue => new OtaPanelInputIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                false);
    }
}
