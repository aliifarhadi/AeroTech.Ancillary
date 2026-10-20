using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice
{
    public static class BackofficeSelectionResultMapper
    {
        public static BackofficeSelectionResultDto Map(AncillarySelectionSnapshot selection)
            => Map(selection, OfferPurchase.CanPurchase(selection.Evaluation), null);

        public static BackofficeSelectionResultDto Map(AncillarySelectionSnapshot selection, bool canPurchase, PricedSelectionToken? pricedSelection)
            => new(
                selection.Shopping.Offer.OfferId,
                EnumValueDto.Of(selection.Evaluation.Status),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                BackofficeAncillaryOfferMapper.Map(selection.CurrentOfferItemRef, selection.Item.PointOfSaleId, selection.Evaluation.Candidate),
                selection.Evaluation.Candidate.Price.RequestedQuantityTotal,
                selection.Evaluation.FieldIssues.Select(issue => new BackofficeFieldIssueDto(issue.Field, issue.ReasonCode)).ToList(),
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
