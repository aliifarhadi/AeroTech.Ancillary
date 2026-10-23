using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Offers;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public sealed class GetAncillaryOffersService : IGetAncillaryOffersService
    {
        private readonly IFlightOfferShoppingSource _source;
        private readonly IAncillaryOfferListing _offers;

        public GetAncillaryOffersService(IFlightOfferShoppingSource source, IAncillaryOfferListing offers)
        {
            _source = source;
            _offers = offers;
        }

        public async Task<AncillaryOffersSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var shopping = await _source.LoadAsync(query.OfferId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            return new AncillaryOffersSnapshot(
                shopping.Offer.OfferId,
                shopping.CurrencyId,
                await _offers.ListAsync(shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).ToList(), OfferFamilyFilters.Services, cancellationToken));
        }
    }
}
