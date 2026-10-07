using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class TravelCriteria : ValueObject
    {
        private const int FlightNumberMaxLength = 16;

        private List<int> _originAirportIds = new();
        private List<int> _destinationAirportIds = new();
        private List<int> _viaAirportIds = new();
        private List<DayOfWeek> _daysOfWeek = new();
        private List<int> _marketingAirlineIds = new();
        private List<int> _operatingAirlineIds = new();
        private List<string> _flightNumbers = new();
        private List<long> _flightIds = new();
        private List<int> _aircraftIds = new();

        private TravelCriteria()
        {
        }

        public IReadOnlyList<int> OriginAirportIds => _originAirportIds.AsReadOnly();

        public IReadOnlyList<int> DestinationAirportIds => _destinationAirportIds.AsReadOnly();

        public IReadOnlyList<int> ViaAirportIds => _viaAirportIds.AsReadOnly();

        public DateOnly? TravelFrom { get; private set; }

        public DateOnly? TravelTo { get; private set; }

        public IReadOnlyList<DayOfWeek> DaysOfWeek => _daysOfWeek.AsReadOnly();

        public TimeOnly? TimeFrom { get; private set; }

        public TimeOnly? TimeTo { get; private set; }

        public IReadOnlyList<int> MarketingAirlineIds => _marketingAirlineIds.AsReadOnly();

        public IReadOnlyList<int> OperatingAirlineIds => _operatingAirlineIds.AsReadOnly();

        public IReadOnlyList<string> FlightNumbers => _flightNumbers.AsReadOnly();

        public IReadOnlyList<long> FlightIds => _flightIds.AsReadOnly();

        public IReadOnlyList<int> AircraftIds => _aircraftIds.AsReadOnly();

        public static TravelCriteria Create(
            IReadOnlyList<int>? originAirportIds,
            IReadOnlyList<int>? destinationAirportIds,
            IReadOnlyList<int>? viaAirportIds,
            DateOnly? travelFrom,
            DateOnly? travelTo,
            IReadOnlyList<DayOfWeek>? daysOfWeek,
            TimeOnly? timeFrom,
            TimeOnly? timeTo,
            IReadOnlyList<int>? marketingAirlineIds,
            IReadOnlyList<int>? operatingAirlineIds,
            IReadOnlyList<string>? flightNumbers,
            IReadOnlyList<long>? flightIds,
            IReadOnlyList<int>? aircraftIds)
        {
            var origins = originAirportIds ?? [];
            var destinations = destinationAirportIds ?? [];
            var vias = viaAirportIds ?? [];
            var days = daysOfWeek ?? [];
            var marketingAirlines = marketingAirlineIds ?? [];
            var operatingAirlines = operatingAirlineIds ?? [];
            var numbers = (flightNumbers ?? []).Select(number => (number ?? string.Empty).Trim().ToUpperInvariant()).ToList();
            var flights = flightIds ?? [];
            var aircraft = aircraftIds ?? [];

            Require(AreIds(origins), nameof(OriginAirportIds));
            Require(AreIds(destinations), nameof(DestinationAirportIds));
            Require(AreIds(vias), nameof(ViaAirportIds));
            Require(travelFrom is null || travelTo is null || travelFrom <= travelTo, nameof(TravelTo));
            Require(days.All(day => Enum.IsDefined(day)) && days.Distinct().Count() == days.Count, nameof(DaysOfWeek));
            Require(timeFrom is null == timeTo is null, nameof(TimeTo));
            Require(AreIds(marketingAirlines), nameof(MarketingAirlineIds));
            Require(AreIds(operatingAirlines), nameof(OperatingAirlineIds));
            Require(
                numbers.All(number => number.Length is >= 1 and <= FlightNumberMaxLength && !number.Any(char.IsWhiteSpace))
                && numbers.Distinct(StringComparer.Ordinal).Count() == numbers.Count,
                nameof(FlightNumbers));
            Require(flights.All(id => id > 0) && flights.Distinct().Count() == flights.Count, nameof(FlightIds));
            Require(AreIds(aircraft), nameof(AircraftIds));

            return new TravelCriteria
            {
                _originAirportIds = origins.ToList(),
                _destinationAirportIds = destinations.ToList(),
                _viaAirportIds = vias.ToList(),
                TravelFrom = travelFrom,
                TravelTo = travelTo,
                _daysOfWeek = days.ToList(),
                TimeFrom = timeFrom,
                TimeTo = timeTo,
                _marketingAirlineIds = marketingAirlines.ToList(),
                _operatingAirlineIds = operatingAirlines.ToList(),
                _flightNumbers = numbers,
                _flightIds = flights.ToList(),
                _aircraftIds = aircraft.ToList()
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return string.Join(',', _originAirportIds);
            yield return string.Join(',', _destinationAirportIds);
            yield return string.Join(',', _viaAirportIds);
            yield return TravelFrom;
            yield return TravelTo;
            yield return string.Join(',', _daysOfWeek);
            yield return TimeFrom;
            yield return TimeTo;
            yield return string.Join(',', _marketingAirlineIds);
            yield return string.Join(',', _operatingAirlineIds);
            yield return string.Join(',', _flightNumbers);
            yield return string.Join(',', _flightIds);
            yield return string.Join(',', _aircraftIds);
        }

        private static bool AreIds(IReadOnlyList<int> ids)
            => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(TravelCriteria)}.{field}");
        }
    }
}
