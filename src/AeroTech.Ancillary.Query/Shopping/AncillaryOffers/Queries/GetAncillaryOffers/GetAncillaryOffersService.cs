using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public sealed class GetAncillaryOffersService : IGetAncillaryOffersService
    {
        private readonly IFlightOfferShoppingSource _source;
        private readonly IAncillaryShoppingEngine _engine;

        public GetAncillaryOffersService(IFlightOfferShoppingSource source, IAncillaryShoppingEngine engine)
        {
            _source = source;
            _engine = engine;
        }

        public Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ExecuteAsync(query, scope, ShoppingFilter.None, cancellationToken);

        public async Task<AncillaryShoppingSnapshot> ExecuteAsync(
            IAncillaryOffersQuery query,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
        {
            var shopping = await _source.LoadAsync(query.OfferId, scope, cancellationToken);
            var pointsOfSale = new List<PointOfSaleShopping>();

            foreach (var (pointOfSaleId, context) in shopping.PointsOfSale)
            {
                var result = await _engine.ShopAsync(context, family, cancellationToken);

                pointsOfSale.Add(new PointOfSaleShopping(
                    pointOfSaleId,
                    context,
                    result,
                    result.Candidates
                        .Select(candidate => new AncillaryOfferItem(
                            OfferItemRef.Of(shopping.Offer.OfferId, context.SourceIdentity.TrustedSourceVersion, scope, pointOfSaleId, candidate.CandidateIdentity),
                            pointOfSaleId,
                            context,
                            candidate))
                        .ToList()));
            }

            return new AncillaryShoppingSnapshot(shopping.Offer, scope, shopping.Facts, shopping.CurrencyId, shopping.EvaluatedAtUtc, pointsOfSale);
        }

        public Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default)
            => FindItemAsync(query, scope, ShoppingFilter.None, cancellationToken);

        public async Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.OfferItemRef))
                throw ExceptionFactory.OfferItemRefIsRequired();

            var shopping = await ExecuteAsync(query, scope, family, cancellationToken);
            var item = shopping.Items.SingleOrDefault(candidate => string.Equals(candidate.OfferItemRef, query.OfferItemRef, StringComparison.Ordinal))
                       ?? throw ExceptionFactory.OfferItemWasNotFound();

            return (shopping, item);
        }
    }
}
