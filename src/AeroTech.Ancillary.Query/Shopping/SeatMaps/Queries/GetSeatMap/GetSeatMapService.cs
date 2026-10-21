using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap
{
    public sealed class GetSeatMapService : IGetSeatMapService
    {
        private const string SeatMapAuthority = "AirInfo";

        private readonly IShoppingSource _source;
        private readonly IAircraftSeatMapProvider _seatMaps;
        private readonly IOccupiedSeatProvider _occupiedSeats;
        private readonly ISeatMapPricingEngine _engine;
        private readonly ICurrencyReference _currencies;
        private readonly IAirportReference _airports;
        private readonly IAirlineReference _airlines;

        public GetSeatMapService(
            IShoppingSource source,
            IAircraftSeatMapProvider seatMaps,
            IOccupiedSeatProvider occupiedSeats,
            ISeatMapPricingEngine engine,
            ICurrencyReference currencies,
            IAirportReference airports,
            IAirlineReference airlines)
        {
            _source = source;
            _seatMaps = seatMaps;
            _occupiedSeats = occupiedSeats;
            _engine = engine;
            _currencies = currencies;
            _airports = airports;
            _airlines = airlines;
        }

        public async Task<SeatMapSnapshot> ExecuteAsync(ISeatMapQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => await BuildAsync(await _source.LoadAsync(query.OfferId, query.OrderId, scope, cancellationToken), scope, cancellationToken);

        public async Task<SeatMapSnapshot> BuildAsync(FlightOfferShopping shopping, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var flights = shopping.Offer.Bounds
                .OrderBy(bound => bound.Sequence)
                .SelectMany(bound => bound.Flights.OrderBy(flight => flight.Sequence).Select(flight => (FlightRef: FlightOfferShoppingContextMapper.FlightRef(bound, flight), Flight: flight)))
                .Where(row => row.Flight.AircraftId is not null)
                .ToList();
            var flightIds = flights.ToDictionary(row => row.FlightRef, row => row.Flight.FlightId, StringComparer.Ordinal);
            var maps = new Dictionary<int, AircraftSeatMap>();

            foreach (var aircraftId in flights.Select(row => row.Flight.AircraftId!.Value).Distinct())
                maps[aircraftId] = await _seatMaps.GetAsync(aircraftId, cancellationToken);

            var occupied = await _occupiedSeats.GetAsync(flights.Select(row => row.Flight.FlightId).Distinct().ToList(), cancellationToken);

            if (flights.Any(row => !occupied.ContainsKey(row.Flight.FlightId)))
                throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the answer misses a requested flight");

            var seats = flights
                .SelectMany(row => Seats(row.FlightRef, maps[row.Flight.AircraftId!.Value], occupied[row.Flight.FlightId]))
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
            var ambiguous = new HashSet<(string TravellerRef, string FlightRef, string SeatNumber)>();
            var pricedContexts = new List<PointOfSaleContext>();

            foreach (var (pointOfSaleId, context) in shopping.PointsOfSale)
            {
                var priced = context with { SeatCharacteristicFacts = facts };
                var result = await _engine.PriceSeatsAsync(priced, OfferFamilyFilters.SeatMap, seats.Select(row => row.Seat).ToList(), cancellationToken);

                pricedContexts.Add(new PointOfSaleContext(pointOfSaleId, priced));

                foreach (var seat in result.Seats)
                {
                    var key = (seat.TravellerRef, seat.FlightRef, seat.SeatNumber.ToUpperInvariant());

                    if (ambiguous.Contains(key))
                        continue;

                    if (offers.TryGetValue(key, out var kept) && kept.Offer.Status == SeatAvailabilityStatus.Available)
                    {
                        if (seat.Status == SeatAvailabilityStatus.Available && !SamePrice(kept.Offer.Price, seat.Candidate?.Price))
                        {
                            ambiguous.Add(key);
                            offers[key] = (seat.FlightRef, kept.Offer with { Status = SeatAvailabilityStatus.Blocked, Id = null, Candidate = null });
                        }

                        continue;
                    }

                    offers[key] = (seat.FlightRef, new SeatTravellerOffer(
                        seat.TravellerRef,
                        seat.SeatNumber,
                        seat.Status,
                        seat.Status == SeatAvailabilityStatus.Available && seat.Candidate?.Price.PricingRevisionId is { } pricingId
                            ? SeatMapSelectionId.Encode(seat.TravellerRef, flightIds[seat.FlightRef], pricingId, seat.SeatNumber)
                            : null,
                        seat.Candidate,
                        pointOfSaleId));
                }
            }

            var currencyCodes = await _currencies.FindCodesAsync([shopping.CurrencyId], cancellationToken);
            var airportCodes = await _airports.FindIataCodesAsync(
                flights.SelectMany(row => new[] { row.Flight.OriginAirportId, row.Flight.DestinationAirportId }).Distinct().ToList(),
                cancellationToken);
            var terminalNumbers = await _airports.FindTerminalNumbersAsync(
                flights.SelectMany(row => new[] { row.Flight.OriginAirportTerminalId, row.Flight.DestinationAirportTerminalId }).OfType<int>().Distinct().ToList(),
                cancellationToken);
            var airlineCodes = await _airlines.FindIataCodesAsync(
                flights.SelectMany(row => new[] { row.Flight.MarketingAirlineId, row.Flight.OperatingAirlineId }).Distinct().ToList(),
                cancellationToken);

            return new SeatMapSnapshot(
                shopping.Offer,
                shopping.CurrencyId,
                currencyCodes.GetValueOrDefault(shopping.CurrencyId) ?? shopping.CurrencyId.ToString(),
                flights
                    .Select(row => new FlightSeatMap(
                        row.FlightRef,
                        row.Flight,
                        new FlightEndpoint(
                            airportCodes.GetValueOrDefault(row.Flight.OriginAirportId),
                            Terminal(terminalNumbers, row.Flight.OriginAirportTerminalId),
                            row.Flight.DepartureAt),
                        new FlightEndpoint(
                            airportCodes.GetValueOrDefault(row.Flight.DestinationAirportId),
                            Terminal(terminalNumbers, row.Flight.DestinationAirportTerminalId),
                            row.Flight.ArrivalAt),
                        airlineCodes.GetValueOrDefault(row.Flight.MarketingAirlineId),
                        airlineCodes.GetValueOrDefault(row.Flight.OperatingAirlineId),
                        row.Flight.AircraftId!.Value,
                        maps[row.Flight.AircraftId!.Value],
                        offers.Values.Where(value => value.FlightRef == row.FlightRef).Select(value => value.Offer).ToList()))
                    .ToList(),
                pricedContexts);
        }

        private static bool SamePrice(PriceAssessment? kept, PriceAssessment? offered)
            => kept?.CurrencyId == offered?.CurrencyId && kept?.CompleteUnitTotal == offered?.CompleteUnitTotal;

        private static string? Terminal(IReadOnlyDictionary<int, string> terminalNumbers, int? terminalId)
            => terminalId is { } id ? terminalNumbers.GetValueOrDefault(id) : null;

        private static IEnumerable<(SeatMapSeat Seat, IReadOnlyList<string> CharacteristicCodes)> Seats(string flightRef, AircraftSeatMap map, IReadOnlySet<string> occupied)
            => map.Decks
                .SelectMany(deck => deck.Cabins)
                .SelectMany(cabin => cabin.Items.Where(item => item.IsSeat).Select(item => (
                    new SeatMapSeat(flightRef, item.Number!.Trim(), cabin.CabinClassId, occupied.Contains(item.Number!.Trim())),
                    item.CharacteristicCodes)));
    }
}
