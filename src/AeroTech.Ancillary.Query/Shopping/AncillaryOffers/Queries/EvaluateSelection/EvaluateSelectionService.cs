using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
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
        private readonly IGetAncillaryOfferDetailService _details;
        private readonly IAncillaryShoppingEngine _engine;

        public EvaluateSelectionService(IGetAncillaryOfferDetailService details, IAncillaryShoppingEngine engine)
        {
            _details = details;
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
            => await EvaluateAsync(await _details.ExecuteAsync(query, scope, family, cancellationToken), selection, cancellationToken);

        public async Task<AncillarySelectionSnapshot> EvaluateAsync(AncillaryOfferDetailSnapshot detail, AncillarySelection selection, CancellationToken cancellationToken = default)
        {
            var item = detail.Item;
            var evaluation = await _engine.EvaluateSelectionAsync(item.Context, item.Candidate.CandidateIdentity, selection, cancellationToken);

            return new AncillarySelectionSnapshot(
                detail.Shopping,
                item,
                evaluation,
                OfferItemRef.Of(detail.Shopping.Offer.OfferId, item.Context.SourceIdentity.TrustedSourceVersion, detail.Shopping.Scope, item.PointOfSaleId, evaluation.Candidate.CandidateIdentity),
                selection,
                detail.OptionGroups);
        }
    }
}
