using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public static class ApiSelectionResultMapper
    {
        public static ApiSelectionResultDto Map(AncillarySelectionSnapshot selection)
            => new(
                selection.Shopping.Offer.OfferId,
                selection.Evaluation.Status.ToString(),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                ApiAncillaryOffersMapper.OfferItem(selection.CurrentOfferItemRef, selection.Evaluation.Candidate),
                ApiAncillaryOffersMapper.Money(selection.Evaluation.Candidate.Price.RequestedQuantityTotal, selection.Evaluation.Candidate.Price.CurrencyId),
                selection.Evaluation.FieldIssues.Select(issue => new ApiSelectionIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                false);
    }
}
