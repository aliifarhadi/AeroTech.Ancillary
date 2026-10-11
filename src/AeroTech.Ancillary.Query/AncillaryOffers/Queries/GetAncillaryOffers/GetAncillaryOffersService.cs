using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.FlightOffers;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public sealed class GetAncillaryOffersService : IGetAncillaryOffersService
    {
        private readonly IFlightOfferProvider _flightOffers;
        private readonly IAirportReference _airports;
        private readonly IAncillaryShoppingEngine _engine;

        public GetAncillaryOffersService(IFlightOfferProvider flightOffers, IAirportReference airports, IAncillaryShoppingEngine engine)
        {
            _flightOffers = flightOffers;
            _airports = airports;
            _engine = engine;
        }

        public async Task<AncillaryOffersDto> ExecuteAsync(IAncillaryOffersQuery query, FlightOfferSalesScope scope, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query.OfferId))
                throw ExceptionFactory.FlightOfferIdIsRequired();

            var offer = await _flightOffers.GetByOfferIdAsync(query.OfferId, cancellationToken);
            var airportIds = offer.Bounds
                .SelectMany(bound => bound.Flights)
                .SelectMany(flight => new[] { flight.OriginAirportId, flight.DestinationAirportId })
                .Distinct()
                .ToList();
            var context = FlightOfferShoppingContextMapper.Map(offer, scope, await _airports.FindCountryIdsAsync(airportIds, cancellationToken));
            var result = await _engine.ShopAsync(context, cancellationToken: cancellationToken);

            return AncillaryOffersMapper.Map(offer, context, result);
        }
    }
}
