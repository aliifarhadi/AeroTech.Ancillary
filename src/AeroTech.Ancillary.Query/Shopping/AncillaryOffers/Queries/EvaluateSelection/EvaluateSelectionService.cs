using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection
{
    public sealed class EvaluateSelectionService : IEvaluateSelectionService
    {
        private readonly IGetAncillaryOffersService _offers;
        private readonly IAncillaryShoppingEngine _engine;

        public EvaluateSelectionService(IGetAncillaryOffersService offers, IAncillaryShoppingEngine engine)
        {
            _offers = offers;
            _engine = engine;
        }

        public Task<AncillarySelectionSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillarySelection selection,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(query, selection, scope, ShoppingFilter.None, cancellationToken);

        public async Task<AncillarySelectionSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillarySelection selection,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
        {
            var (shopping, item) = await _offers.FindItemAsync(query, scope, family, cancellationToken);
            var evaluation = await _engine.EvaluateSelectionAsync(item.Context, item.Candidate.CandidateIdentity, selection, cancellationToken);

            return new AncillarySelectionSnapshot(
                shopping,
                item,
                evaluation,
                OfferItemRef.Of(shopping.Offer.OfferId, scope, item.PointOfSaleId, evaluation.Candidate.CandidateIdentity));
        }
    }
}
