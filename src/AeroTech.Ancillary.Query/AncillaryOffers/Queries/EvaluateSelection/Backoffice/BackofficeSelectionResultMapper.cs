using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Backoffice
{
    public static class BackofficeSelectionResultMapper
    {
        public static BackofficeSelectionResultDto Map(AncillarySelectionSnapshot selection)
            => new(
                selection.Shopping.Offer.OfferId,
                EnumValueDto.Of(selection.Evaluation.Status),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                BackofficeAncillaryOfferMapper.Map(selection.CurrentOfferItemRef, selection.Item.PointOfSaleId, selection.Evaluation.Candidate),
                selection.Evaluation.Candidate.Price.RequestedQuantityTotal,
                selection.Evaluation.FieldIssues.Select(issue => new BackofficeFieldIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                false);
    }
}
