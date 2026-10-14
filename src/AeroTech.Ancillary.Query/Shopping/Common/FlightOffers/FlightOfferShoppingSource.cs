using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public sealed class FlightOfferShoppingSource : IFlightOfferShoppingSource
    {
        private readonly IFlightOfferProvider _flightOffers;
        private readonly IAirportReference _airports;
        private readonly IAirFareReference _airFares;

        public FlightOfferShoppingSource(IFlightOfferProvider flightOffers, IAirportReference airports, IAirFareReference airFares)
        {
            _flightOffers = flightOffers;
            _airports = airports;
            _airFares = airFares;
        }

        public async Task<FlightOfferShopping> LoadAsync(string? offerId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(offerId))
                throw ExceptionFactory.FlightOfferIdIsRequired();

            var offer = await _flightOffers.GetByOfferIdAsync(offerId, cancellationToken);
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

            return new FlightOfferShopping(
                offer,
                facts,
                FlightOfferShoppingContextMapper.CurrencyOf(offer, scope),
                scope.PointOfSaleIds
                    .Distinct()
                    .Order()
                    .Select(pointOfSaleId => new PointOfSaleContext(
                        pointOfSaleId,
                        FlightOfferShoppingContextMapper.Map(offer, facts, scope, pointOfSaleId, airportCountryIds, airportTimeZoneIds)))
                    .ToList());
        }
    }
}
