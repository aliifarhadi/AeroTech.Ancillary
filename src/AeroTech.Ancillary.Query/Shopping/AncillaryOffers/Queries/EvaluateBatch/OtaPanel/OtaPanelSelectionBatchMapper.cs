using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel
{
    public static class OtaPanelSelectionBatchMapper
    {
        public static OtaPanelSelectionBatchDto Map(SelectionBatchSnapshot batch)
            => new(batch.Shopping.Offer.OfferId, batch.Lines.Select(Line).ToList(), OfferSelectionBatchMapper.Basket(batch.Basket));

        private static OtaPanelSelectionLineDto Line(SelectionLineSnapshot line)
            => new(
                line.ClientSelectionId,
                line.Status.ToString(),
                line.ReasonCodes,
                line.CanPurchase,
                line.PricedSelection?.Value,
                line.PricedSelection?.ExpiresAtUtc,
                line.Ancillary is { } ancillary ? OtaPanelSelectionResultMapper.Map(ancillary, line.CanPurchase, line.PricedSelection) : null);
    }
}
