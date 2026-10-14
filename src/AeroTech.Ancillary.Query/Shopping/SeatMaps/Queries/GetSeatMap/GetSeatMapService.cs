using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap
{
    public sealed class GetSeatMapService : IGetSeatMapService
    {
        private const string SeatMapAuthority = "AirInfo";

        private readonly IFlightOfferShoppingSource _source;
        private readonly IAircraftSeatMapProvider _seatMaps;
        private readonly IOccupiedSeatProvider _occupiedSeats;
        private readonly ISeatMapPricingEngine _engine;
        private readonly ICurrencyReference _currencies;

        public GetSeatMapService(
            IFlightOfferShoppingSource source,
            IAircraftSeatMapProvider seatMaps,
            IOccupiedSeatProvider occupiedSeats,
            ISeatMapPricingEngine engine,
            ICurrencyReference currencies)
        {
            _source = source;
            _seatMaps = seatMaps;
            _occupiedSeats = occupiedSeats;
            _engine = engine;
            _currencies = currencies;
        }

        public async Task<SeatMapSnapshot> ExecuteAsync(ISeatMapQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var shopping = await _source.LoadAsync(query.OfferId, scope, cancellationToken);
            var flights = shopping.Offer.Bounds
                .OrderBy(bound => bound.Sequence)
                .SelectMany(bound => bound.Flights.OrderBy(flight => flight.Sequence).Select(flight => (FlightRef: FlightOfferShoppingContextMapper.FlightRef(bound, flight), Flight: flight)))
                .Where(row => row.Flight.AircraftId is not null)
                .ToList();
            var maps = new Dictionary<int, AircraftSeatMap>();

            foreach (var aircraftId in flights.Select(row => row.Flight.AircraftId!.Value).Distinct())
                maps[aircraftId] = await _seatMaps.GetAsync(aircraftId, cancellationToken);

            var occupied = await _occupiedSeats.GetAsync(flights.Select(row => row.Flight.FlightId).Distinct().ToList(), cancellationToken);
            var seats = flights
                .SelectMany(row => Seats(row.FlightRef, maps[row.Flight.AircraftId!.Value], occupied.GetValueOrDefault(row.Flight.FlightId)))
                .DistinctBy(row => (row.Seat.FlightRef, row.Seat.SeatNumber.ToUpperInvariant()))
                .ToList();
            var facts = seats
                .Select(row => new SeatCharacteristicFacts
                {
                    FlightRef = row.Seat.FlightRef,
                    SeatNumber = row.Seat.SeatNumber,
                    CharacteristicCodes = row.CharacteristicCodes,
                    SourceCompleteness = FactEvidence.Verified,
                    SourceAuthority = SeatMapAuthority
                })
                .ToList();
            var offers = new Dictionary<(string TravellerRef, string FlightRef, string SeatNumber), (string FlightRef, SeatTravellerOffer Offer)>();

            foreach (var (pointOfSaleId, context) in shopping.PointsOfSale)
            {
                var result = await _engine.PriceSeatsAsync(
                    context with { SeatCharacteristicFacts = facts },
                    OfferFamilyFilters.SeatMap,
                    seats.Select(row => row.Seat).ToList(),
                    cancellationToken);

                foreach (var seat in result.Seats)
                {
                    var key = (seat.TravellerRef, seat.FlightRef, seat.SeatNumber.ToUpperInvariant());

                    if (offers.TryGetValue(key, out var kept) && kept.Offer.Status == SeatAvailabilityStatus.Available)
                        continue;

                    offers[key] = (seat.FlightRef, new SeatTravellerOffer(
                        seat.TravellerRef,
                        seat.SeatNumber,
                        seat.Status,
                        seat.Status == SeatAvailabilityStatus.Available
                            ? OfferItemRef.OfSeat(shopping.Offer.OfferId, scope, pointOfSaleId, seat.TravellerRef, seat.FlightRef, seat.SeatNumber, seat.Candidate?.CandidateIdentity)
                            : null,
                        seat.Candidate?.Price));
                }
            }

            var currencyCodes = await _currencies.FindCodesAsync([shopping.CurrencyId], cancellationToken);

            return new SeatMapSnapshot(
                shopping.Offer,
                shopping.CurrencyId,
                currencyCodes.GetValueOrDefault(shopping.CurrencyId) ?? shopping.CurrencyId.ToString(),
                flights
                    .Select(row => new FlightSeatMap(
                        row.FlightRef,
                        row.Flight,
                        row.Flight.AircraftId!.Value,
                        maps[row.Flight.AircraftId!.Value],
                        offers.Values.Where(value => value.FlightRef == row.FlightRef).Select(value => value.Offer).ToList()))
                    .ToList());
        }

        private static IEnumerable<(SeatMapSeat Seat, IReadOnlyList<string> CharacteristicCodes)> Seats(string flightRef, AircraftSeatMap map, IReadOnlySet<string>? occupied)
            => map.Decks
                .SelectMany(deck => deck.Cabins)
                .SelectMany(cabin => cabin.Items.Where(item => item.IsSeat).Select(item => (
                    new SeatMapSeat(flightRef, item.Number!.Trim(), cabin.CabinClassId, occupied?.Contains(item.Number!.Trim()) == true),
                    item.CharacteristicCodes)));
    }
}
