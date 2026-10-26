using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
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

        public Task<AncillaryOffersSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ListAsync(query.OfferId, scope, null, cancellationToken);

        public Task<AncillaryOffersSnapshot> ExecuteAsync(SelectionSnapshot selection, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ListAsync(selection.OfferId, scope, selection, cancellationToken);

        private async Task<AncillaryOffersSnapshot> ListAsync(
            string? offerId,
            AncillarySalesScope scope,
            SelectionSnapshot? selection,
            CancellationToken cancellationToken)
        {
            var shopping = await _source.LoadAsync(offerId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            return new AncillaryOffersSnapshot(
                shopping.Offer.OfferId,
                shopping.CurrencyId,
                await _offers.ListAsync(shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).ToList(), OfferFamilyFilters.Services, selection, cancellationToken));
        }
    }
}
