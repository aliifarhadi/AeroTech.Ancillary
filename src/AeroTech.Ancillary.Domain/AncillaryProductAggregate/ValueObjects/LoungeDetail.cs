using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects
{
    public sealed class LoungeDetail : ValueObject
    {
        private readonly List<int> _airportIds = new();

        private LoungeDetail()
        {
        }

        public LoungeDetail(IReadOnlyCollection<int> airportIds)
        {
            if (airportIds.Count == 0
                || airportIds.Distinct().Count() != airportIds.Count
                || airportIds.Any(airportId => airportId <= 0))
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(LoungeDetail)}.{nameof(AirportIds)}");

            _airportIds.AddRange(airportIds);
        }

        public IReadOnlyCollection<int> AirportIds => _airportIds.AsReadOnly();

        public bool IsOfferedAt(int airportId) => _airportIds.Contains(airportId);

        public LoungeDetail Copy() => new(_airportIds);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            foreach (var airportId in _airportIds)
                yield return airportId;
        }
    }
}
