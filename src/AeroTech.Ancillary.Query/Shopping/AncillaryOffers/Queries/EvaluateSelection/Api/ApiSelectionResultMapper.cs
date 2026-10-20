using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public static class ApiSelectionResultMapper
    {
        public const string AncillarySelection = "ancillary-selection";

        public static ApiSelectionResultDto Map(AncillarySelectionSnapshot selection, ApiOfferReferences references)
            => new(Data(selection, references, OfferPurchase.CanPurchase(selection.Evaluation), null));

        public static ApiSelectionResultDataDto Data(AncillarySelectionSnapshot selection, ApiOfferReferences references, bool canPurchase, PricedSelectionToken? pricedSelection)
        {
            var candidate = selection.Evaluation.Candidate;
            var bag = ApiAncillaryOffersMapper.IsBag(candidate);

            return new ApiSelectionResultDataDto(
                AncillarySelection,
                selection.Shopping.Offer.OfferId,
                selection.Evaluation.Status.ToString(),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                bag ? ApiAncillaryOffersMapper.Bag(selection.CurrentOfferItemRef, candidate, selection.OptionGroups, references) : null,
                bag ? null : ApiAncillaryOffersMapper.OtherService(selection.CurrentOfferItemRef, candidate, selection.OptionGroups, references),
                ApiAncillaryOffersMapper.Price(candidate.Price.RequestedQuantityTotal, candidate.Price.CurrencyId, references),
                selection.Evaluation.FieldIssues.Select(issue => new ApiSelectionIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                canPurchase,
                pricedSelection?.Value,
                pricedSelection?.ExpiresAtUtc,
                OfferSelectionResultsMapper.SelectedOptions(selection),
                OfferSelectionResultsMapper.Beneficiary(selection, references.TravelerIds),
                OfferSelectionResultsMapper.Coverage(selection, references.SegmentIds),
                OfferSelectionResultsMapper.ExactPrice(candidate.Price, canPurchase),
                candidate.Availability.RequiresCheckAtReserve);
        }
    }
}
