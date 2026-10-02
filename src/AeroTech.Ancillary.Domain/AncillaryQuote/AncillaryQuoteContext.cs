using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryQuote
{
    internal sealed class AncillaryQuoteContext
    {
        private readonly Dictionary<string, AncillaryQuoteTraveller> _travellers;
        private readonly Dictionary<string, AncillaryQuoteBound> _bounds;
        private readonly Dictionary<string, AncillaryQuoteFlight> _flights;
        private readonly Dictionary<string, AncillaryQuoteBound> _boundsByFlight;
        private readonly Dictionary<string, PassengerTypeCode> _passengerTypes;
        private readonly IReadOnlyList<AncillaryQuoteExistingOccurrence> _existing;

        private AncillaryQuoteContext(
            Dictionary<string, AncillaryQuoteTraveller> travellers,
            Dictionary<string, AncillaryQuoteBound> bounds,
            Dictionary<string, AncillaryQuoteFlight> flights,
            Dictionary<string, AncillaryQuoteBound> boundsByFlight,
            Dictionary<string, PassengerTypeCode> passengerTypes,
            IReadOnlyList<AncillaryQuoteExistingOccurrence> existing)
        {
            _travellers = travellers;
            _bounds = bounds;
            _flights = flights;
            _boundsByFlight = boundsByFlight;
            _passengerTypes = passengerTypes;
            _existing = existing;
        }

        public static AncillaryQuoteContext Of(AncillaryQuoteRequest request)
        {
            var flights = request.Bounds.SelectMany(bound => bound.Flights).ToList();

            Consistent(IsDistinct(request.Travellers.Select(traveller => traveller.Ref)), nameof(request.Travellers));
            Consistent(IsDistinct(request.Bounds.Select(bound => bound.Ref)), nameof(request.Bounds));
            Consistent(IsDistinct(flights.Select(flight => flight.Ref)), nameof(AncillaryQuoteBound.Flights));

            var flightsByRef = flights.ToDictionary(flight => flight.Ref, StringComparer.Ordinal);
            var passengerTypes = new Dictionary<string, PassengerTypeCode>(StringComparer.Ordinal);

            foreach (var traveller in request.Travellers)
            {
                Consistent(
                    Enum.GetNames<PassengerTypeCode>().Contains(traveller.PassengerTypeCode, StringComparer.Ordinal),
                    nameof(traveller.PassengerTypeCode));
                Consistent(
                    IsDistinct(traveller.FlightRefs) && traveller.FlightRefs.All(flightsByRef.ContainsKey),
                    nameof(traveller.FlightRefs));

                passengerTypes[traveller.Ref] = Enum.Parse<PassengerTypeCode>(traveller.PassengerTypeCode);
            }

            var context = new AncillaryQuoteContext(
                request.Travellers.ToDictionary(traveller => traveller.Ref, StringComparer.Ordinal),
                request.Bounds.ToDictionary(bound => bound.Ref, StringComparer.Ordinal),
                flightsByRef,
                request.Bounds
                    .SelectMany(bound => bound.Flights.Select(flight => (flight.Ref, Bound: bound)))
                    .ToDictionary(flight => flight.Ref, flight => flight.Bound, StringComparer.Ordinal),
                passengerTypes,
                request.Existing);

            foreach (var existing in request.Existing)
            {
                context.Traveller(existing.TravellerRef);

                if (existing.BoundRef is not null)
                    context.Bound(existing.BoundRef);

                if (existing.FlightRef is not null)
                    context.Flight(existing.FlightRef);
            }

            return context;
        }

        public AncillaryQuoteTraveller Traveller(string travellerRef)
            => _travellers.GetValueOrDefault(travellerRef) ?? throw ExceptionFactory.AncillaryQuoteReferenceNotInRequest(travellerRef);

        public AncillaryQuoteBound Bound(string boundRef)
            => _bounds.GetValueOrDefault(boundRef) ?? throw ExceptionFactory.AncillaryQuoteReferenceNotInRequest(boundRef);

        public AncillaryQuoteFlight Flight(string flightRef)
            => _flights.GetValueOrDefault(flightRef) ?? throw ExceptionFactory.AncillaryQuoteReferenceNotInRequest(flightRef);

        public PassengerTypeCode PassengerTypeOf(AncillaryQuoteTraveller traveller) => _passengerTypes[traveller.Ref];

        public IReadOnlyList<AncillaryQuoteFlight> CoveredFlights(AncillaryQuoteTraveller traveller, AncillaryQuoteBound bound)
            => bound.Flights.Where(flight => traveller.FlightRefs.Contains(flight.Ref, StringComparer.Ordinal)).ToList();

        public AncillaryQuoteBound BoundOf(AncillaryQuoteFlight flight) => _boundsByFlight[flight.Ref];

        public int ExistingQuantity(string productRef, string travellerRef, AncillaryQuoteBound bound, AncillaryQuoteFlight? flight)
            => _existing
                .Where(existing => existing.ProductRef == productRef
                                   && existing.TravellerRef == travellerRef
                                   && (flight is null ? existing.BoundRef == bound.Ref : existing.FlightRef == flight.Ref))
                .Sum(existing => existing.Quantity);

        private static bool IsDistinct(IEnumerable<string> values)
        {
            var list = values.ToList();

            return list.Distinct(StringComparer.Ordinal).Count() == list.Count;
        }

        private static void Consistent(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryQuoteRequestIsInconsistent(field);
        }
    }
}
