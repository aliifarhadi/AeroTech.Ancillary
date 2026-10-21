using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public static class OfferSelectionBatchMapper
    {
        public static OfferBasketDto Basket(SelectionBasket basket)
            => new(basket.AllReady, basket.CurrencyId, basket.Total, basket.SharedFees.Select(OfferSelectionResultsMapper.Line).ToList(), basket.ReasonCodes);
    }
}
