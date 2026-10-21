using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api
{
    public static class ApiSelectionBatchMapper
    {
        public const string AncillarySelectionBatch = "ancillary-selection-batch";

        public static ApiSelectionBatchDto Map(SelectionBatchSnapshot batch, ApiOfferReferences references)
            => new(new ApiSelectionBatchDataDto(
                AncillarySelectionBatch,
                batch.Shopping.Offer.OfferId,
                batch.Lines.Select(line => Line(line, references)).ToList(),
                OfferSelectionBatchMapper.Basket(batch.Basket)));

        private static ApiSelectionLineDto Line(SelectionLineSnapshot line, ApiOfferReferences references)
            => new(
                line.ClientSelectionId,
                line.Status.ToString(),
                line.ReasonCodes,
                line.CanPurchase,
                line.PricedSelection?.Value,
                line.PricedSelection?.ExpiresAtUtc,
                line.Ancillary is { } ancillary ? ApiSelectionResultMapper.Data(ancillary, references, line.CanPurchase, line.PricedSelection) : null);
    }
}
