using System.Globalization;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiOfferReferences
    {
        private readonly Dictionary<string, string> _travelerIds;
        private readonly Dictionary<string, IReadOnlyList<string>> _segmentIds = new(StringComparer.Ordinal);
        private readonly IReadOnlyDictionary<int, string> _currencyCodes;
        private readonly IReadOnlyDictionary<long, BaggageSpecificationArgs> _baggage;

        public ApiOfferReferences(
            FlightOffer offer,
            IReadOnlyDictionary<int, string> airportCodes,
            IReadOnlyDictionary<int, string> airlineCodes,
            IReadOnlyDictionary<int, string> currencyCodes,
            IReadOnlyDictionary<long, BaggageSpecificationArgs> baggage)
        {
            _currencyCodes = currencyCodes;
            _baggage = baggage;
            _travelerIds = offer.Tickets.ToDictionary(ticket => ticket.TravellerRef, ticket => Id(ticket.TravellerIndex), StringComparer.Ordinal);
            Travelers = offer.Tickets
                .OrderBy(ticket => ticket.TravellerIndex)
                .Select(ticket => new ApiTravelerDto(Id(ticket.TravellerIndex), TravelerType(ticket.PassengerTypeCode)))
                .ToList();

            var segments = new List<ApiSegmentDto>();

            foreach (var bound in offer.Bounds.OrderBy(bound => bound.Sequence))
            {
                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                {
                    var ids = new List<string>();

                    foreach (var leg in flight.Legs.OrderBy(leg => leg.Sequence))
                    {
                        ids.Add(Id(segments.Count + 1));
                        segments.Add(new ApiSegmentDto(
                            ids[^1],
                            airlineCodes.GetValueOrDefault(flight.MarketingAirlineId),
                            flight.FlightNumber,
                            new ApiEndpointDto(airportCodes.GetValueOrDefault(leg.OriginAirportId), leg.DepartureAt),
                            new ApiEndpointDto(airportCodes.GetValueOrDefault(leg.DestinationAirportId), leg.ArrivalAt),
                            new ApiOperatingDto(airlineCodes.GetValueOrDefault(flight.OperatingAirlineId))));
                    }

                    _segmentIds[FlightOfferShoppingContextMapper.FlightRef(bound, flight)] = ids;
                }
            }

            Segments = segments;
        }

        public IReadOnlyList<ApiTravelerDto> Travelers { get; }

        public IReadOnlyList<ApiSegmentDto> Segments { get; }

        public IReadOnlyList<string> TravelerIds(IEnumerable<string> travellerRefs)
            => travellerRefs.Select(travellerRef => _travelerIds[travellerRef]).ToList();

        public IReadOnlyList<string> SegmentIds(IEnumerable<string> flightRefs)
            => flightRefs.SelectMany(flightRef => _segmentIds[flightRef]).ToList();

        public string? CurrencyCode(int? currencyId)
            => currencyId is { } id ? _currencyCodes.GetValueOrDefault(id) : null;

        public BaggageSpecificationArgs? Baggage(long definitionVersionId) => _baggage.GetValueOrDefault(definitionVersionId);

        public static string Amount(decimal amount) => amount.ToString("0.00####", CultureInfo.InvariantCulture);

        private static string Id(int number) => number.ToString(CultureInfo.InvariantCulture);

        private static string TravelerType(PassengerTypeCode passengerTypeCode)
            => passengerTypeCode switch
            {
                PassengerTypeCode.ADT => "ADULT",
                PassengerTypeCode.CHD => "CHILD",
                PassengerTypeCode.INF => "INFANT",
                _ => passengerTypeCode.ToString()
            };
    }
}
