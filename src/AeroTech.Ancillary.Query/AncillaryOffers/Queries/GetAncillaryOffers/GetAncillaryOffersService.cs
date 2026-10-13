using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public sealed class GetAncillaryOffersService : IGetAncillaryOffersService
    {
        private readonly IFlightOfferProvider _flightOffers;
        private readonly IAirportReference _airports;
        private readonly IAirFareReference _airFares;
        private readonly IAncillaryShoppingEngine _engine;

        public GetAncillaryOffersService(
            IFlightOfferProvider flightOffers,
            IAirportReference airports,
            IAirFareReference airFares,
            IAncillaryShoppingEngine engine)
        {
            _flightOffers = flightOffers;
            _airports = airports;
            _airFares = airFares;
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
            if (string.IsNullOrWhiteSpace(query.OfferId))
                throw ExceptionFactory.FlightOfferIdIsRequired();

            var offer = await _flightOffers.GetByOfferIdAsync(query.OfferId, cancellationToken);
            var airFareIds = offer.FareComponents.Select(component => component.AirFareId).Distinct().ToList();
            var fareFamilyIds = await _airFares.FindFareFamilyIdsAsync(airFareIds, cancellationToken);
            var facts = FlightOfferShoppingContextMapper.Facts(offer, fareFamilyIds);
            var airportIds = offer.Bounds
                .SelectMany(bound => bound.Flights)
                .SelectMany(flight => new[] { flight.OriginAirportId, flight.DestinationAirportId })
                .Distinct()
                .ToList();
            var airportCountryIds = await _airports.FindCountryIdsAsync(airportIds, cancellationToken);
            var airportTimeZoneIds = await _airports.FindTimeZoneIdsAsync(airportIds, cancellationToken);
            var pointsOfSale = new List<PointOfSaleShopping>();

            foreach (var pointOfSaleId in scope.PointOfSaleIds.Distinct().Order())
            {
                var context = FlightOfferShoppingContextMapper.Map(offer, facts, scope, pointOfSaleId, airportCountryIds, airportTimeZoneIds);
                var result = await _engine.ShopAsync(context, family, cancellationToken);

                pointsOfSale.Add(new PointOfSaleShopping(
                    pointOfSaleId,
                    context,
                    result,
                    result.Candidates
                        .Select(candidate => new AncillaryOfferItem(
                            OfferItemRef.Of(offer.OfferId, scope, pointOfSaleId, candidate.CandidateIdentity),
                            pointOfSaleId,
                            context,
                            candidate))
                        .ToList()));
            }

            return new AncillaryShoppingSnapshot(offer, scope, facts, FlightOfferShoppingContextMapper.CurrencyOf(offer, scope), pointsOfSale);
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
