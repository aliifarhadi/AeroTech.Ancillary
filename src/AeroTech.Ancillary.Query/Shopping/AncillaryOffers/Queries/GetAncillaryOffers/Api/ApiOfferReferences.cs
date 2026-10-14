using System.Globalization;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiOfferReferences
    {
        private readonly IReadOnlyDictionary<string, string> _travelerIds;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _segmentIds;
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
            _travelerIds = ApiOfferIdentifiers.TravelerIds(offer);
            Travelers = offer.Tickets
                .OrderBy(ticket => ticket.TravellerIndex)
                .Select(ticket => new ApiTravelerDto(ApiOfferIdentifiers.Id(ticket.TravellerIndex), TravelerType(ticket.PassengerTypeCode)))
                .ToList();

            var legs = ApiOfferIdentifiers.Segments(offer);

            _segmentIds = legs
                .GroupBy(segment => segment.FlightRef, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(segment => segment.Id).ToList(), StringComparer.Ordinal);
            Segments = legs
                .Select(segment => new ApiSegmentDto(
                    segment.Id,
                    airlineCodes.GetValueOrDefault(segment.Flight.MarketingAirlineId),
                    segment.Flight.FlightNumber,
                    new ApiEndpointDto(airportCodes.GetValueOrDefault(segment.Leg.OriginAirportId), segment.Leg.DepartureAt),
                    new ApiEndpointDto(airportCodes.GetValueOrDefault(segment.Leg.DestinationAirportId), segment.Leg.ArrivalAt),
                    new ApiOperatingDto(airlineCodes.GetValueOrDefault(segment.Flight.OperatingAirlineId))))
                .ToList();
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
