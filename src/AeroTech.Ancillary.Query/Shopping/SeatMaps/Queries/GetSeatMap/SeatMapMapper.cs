using System.Globalization;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.SeatMaps;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap
{
    public static class SeatMapMapper
    {
        public const string SeatMapType = "seatmap";

        private const string Zero = "0";

        public static SeatMapsDto Map(SeatMapSnapshot snapshot, Func<string, string> travelerId, Func<FlightSeatMap, string> segmentId)
        {
            var travellers = snapshot.Offer.Tickets.OrderBy(ticket => ticket.TravellerIndex).Select(ticket => ticket.TravellerRef).ToList();
            var maps = snapshot.Flights.Select(flight => SeatMap(snapshot, flight, travellers, travelerId, segmentId)).ToList();

            return new SeatMapsDto(
                new SeatMapMetaDto(maps.Count),
                maps,
                new SeatMapDictionariesDto(
                    Merge(snapshot.Flights.Select(flight => flight.Map.Facilities)),
                    Merge(snapshot.Flights.Select(flight => flight.Map.SeatCharacteristics))));
        }

        public static OrderSeatMapsDto ForOrder(long orderId, SeatMapsDto seatMaps)
            => new(
                seatMaps.Meta,
                seatMaps.Data
                    .Select(map => new OrderSeatMapDto(
                        map.Type,
                        orderId,
                        map.SegmentId,
                        map.Departure,
                        map.Arrival,
                        map.CarrierCode,
                        map.Number,
                        map.Operating,
                        map.AircraftId,
                        map.Class,
                        map.CabinClassId,
                        map.Decks,
                        map.AvailableSeatsCounters))
                    .ToList(),
                seatMaps.Dictionaries);

        private static SeatMapDto SeatMap(
            SeatMapSnapshot snapshot,
            FlightSeatMap flight,
            IReadOnlyList<string> travellers,
            Func<string, string> travelerId,
            Func<FlightSeatMap, string> segmentId)
        {
            var decks = flight.Map.Decks
                .Select(deck => new SeatMapDeckDto(
                    deck.DeckType,
                    new SeatMapDeckConfigurationDto(
                        deck.Configuration.Width,
                        deck.Configuration.Length,
                        deck.Configuration.StartSeatRow,
                        deck.Configuration.EndSeatRow,
                        deck.Configuration.StartWingsX,
                        deck.Configuration.EndWingsX,
                        deck.Configuration.StartWingsRow,
                        deck.Configuration.EndWingsRow,
                        deck.Configuration.ExitRowsX),
                    deck.Cabins
                        .Select(cabin => new SeatMapCabinDto(
                            cabin.CabinClass,
                            cabin.CabinClassId,
                            cabin.StartSeatRow,
                            cabin.EndSeatRow,
                            cabin.Items.Select(item => Item(snapshot, flight, item, travellers, travelerId)).ToList()))
                        .ToList()))
                .ToList();

            return new SeatMapDto(
                SeatMapType,
                snapshot.Offer.OfferId,
                segmentId(flight),
                Endpoint(flight.Departure),
                Endpoint(flight.Arrival),
                flight.CarrierCode,
                flight.Flight.FlightNumber,
                new SeatMapOperatingDto(flight.OperatingCarrierCode),
                flight.AircraftId,
                flight.Flight.BookingClass,
                flight.Flight.CabinClassId,
                decks,
                travellers
                    .Select(traveller => new SeatMapAvailableSeatsCounterDto(
                        travelerId(traveller),
                        flight.Offers.Count(offer => offer.TravellerRef == traveller && offer.Status == SeatAvailabilityStatus.Available)))
                    .ToList());
        }

        private static SeatMapEndpointDto Endpoint(FlightEndpoint endpoint) => new(endpoint.IataCode, endpoint.Terminal, endpoint.At);

        private static SeatMapItemDto Item(
            SeatMapSnapshot snapshot,
            FlightSeatMap flight,
            AircraftCabinItem item,
            IReadOnlyList<string> travellers,
            Func<string, string> travelerId)
            => new(
                item.Number,
                item.Code,
                item.Column,
                item.Row,
                item.Title,
                item.CharacteristicCodes,
                item.Conditions,
                new SeatMapCoordinatesDto(item.X, item.Y, item.Length),
                item.IsSeat ? TravelerPricing(snapshot, flight.OffersOf(item.Number!), travellers, travelerId) : []);

        private static IReadOnlyList<SeatMapTravelerPricingDto> TravelerPricing(
            SeatMapSnapshot snapshot,
            IReadOnlyList<SeatTravellerOffer> offers,
            IReadOnlyList<string> travellers,
            Func<string, string> travelerId)
            => travellers
                .Select(traveller => offers.FirstOrDefault(offer => offer.TravellerRef == traveller) is { } offer
                    ? new SeatMapTravelerPricingDto(
                        offer.Id,
                        travelerId(traveller),
                        Status(offer.Status),
                        offer.Status == SeatAvailabilityStatus.Available ? Price(snapshot, offer.Price) : null)
                    : new SeatMapTravelerPricingDto(null, travelerId(traveller), Status(SeatAvailabilityStatus.Blocked), null))
                .ToList();

        private static SeatMapPriceDto Price(SeatMapSnapshot snapshot, PriceAssessment? price)
            => price is null
                ? new SeatMapPriceDto(snapshot.CurrencyCode, Zero, Zero, [], [])
                : new SeatMapPriceDto(
                    snapshot.CurrencyCode,
                    Amount(price.BaseAmount ?? 0m),
                    Amount(price.CompleteUnitTotal ?? 0m),
                    price.AddedTaxLines.Select(line => new SeatMapTaxDto(Amount(line.Amount), line.Code)).ToList(),
                    price.AppliedUnitFeeLines.Concat(price.UnappliedFeeLines).Select(line => new SeatMapFeeDto(Amount(line.Amount), line.Code)).ToList());

        private static string Status(SeatAvailabilityStatus status)
            => status switch
            {
                SeatAvailabilityStatus.Available => "AVAILABLE",
                SeatAvailabilityStatus.Occupied => "OCCUPIED",
                _ => "BLOCKED"
            };

        private static string Amount(decimal amount) => amount.ToString("0.00####", CultureInfo.InvariantCulture);

        private static IReadOnlyDictionary<string, string> Merge(IEnumerable<IReadOnlyDictionary<string, string>> dictionaries)
        {
            var merged = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var (code, title) in dictionaries.SelectMany(dictionary => dictionary))
                merged.TryAdd(code, title);

            return merged;
        }
    }
}
