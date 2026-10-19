using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public static class ApiSelectionResultMapper
    {
        public const string AncillarySelection = "ancillary-selection";

        public static ApiSelectionResultDto Map(AncillarySelectionSnapshot selection, ApiOfferReferences references)
        {
            var candidate = selection.Evaluation.Candidate;
            var bag = ApiAncillaryOffersMapper.IsBag(candidate);

            return new ApiSelectionResultDto(
                new ApiSelectionResultDataDto(
                    AncillarySelection,
                    selection.Shopping.Offer.OfferId,
                    selection.Evaluation.Status.ToString(),
                    selection.Item.OfferItemRef,
                    selection.CurrentOfferItemRef,
                    selection.Evaluation.ProvisionChangedBySelection,
                    bag ? ApiAncillaryOffersMapper.Bag(candidate, references) : null,
                    bag ? null : ApiAncillaryOffersMapper.OtherService(candidate, references),
                    ApiAncillaryOffersMapper.Price(candidate.Price.RequestedQuantityTotal, candidate.Price.CurrencyId, references),
                    selection.Evaluation.FieldIssues.Select(issue => new ApiSelectionIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                    OfferPurchase.CanPurchase(selection.Evaluation),
                    null,
                    null,
                    OfferSelectionResultsMapper.SelectedOptions(selection),
                    OfferSelectionResultsMapper.Beneficiary(selection, references.TravelerIds),
                    OfferSelectionResultsMapper.Coverage(selection, references.SegmentIds),
                    OfferSelectionResultsMapper.ExactPrice(selection),
                    candidate.Availability.RequiresCheckAtReserve));
        }
    }
}
