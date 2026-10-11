using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap
{
    public sealed class GetSeatMapService : IGetSeatMapService
    {
        private readonly IGetAncillaryOffersService _offers;
        private readonly IFlightSeatMapReference _seatMaps;

        public GetSeatMapService(IGetAncillaryOffersService offers, IFlightSeatMapReference seatMaps)
        {
            _offers = offers;
            _seatMaps = seatMaps;
        }

        public async Task ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var (_, item) = await _offers.FindItemAsync(query, scope, cancellationToken);

            if (item.Candidate.Profile != AncillaryProfile.Seat)
                throw ExceptionFactory.ShoppingContextIsInvalid(nameof(query.OfferItemRef));

            var flights = item.Context.Flights.Where(flight => item.Candidate.FlightRefs.Contains(flight.FlightRef)).ToList();
            var answers = new List<string>();

            foreach (var flight in flights)
                answers.Add($"{flight.FlightRef}: {await _seatMaps.CheckAsync(scope.OwnerAirlineId, flight.FlightId, cancellationToken)}");

            throw ExceptionFactory.SeatMapSourceUnavailable(string.Join(", ", answers));
        }
    }
}
