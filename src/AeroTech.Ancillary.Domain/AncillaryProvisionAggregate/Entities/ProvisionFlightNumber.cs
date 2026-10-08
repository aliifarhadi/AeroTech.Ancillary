using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFlightNumber : Entity<long>
    {
        private const int MaxLength = 16;

        private ProvisionFlightNumber()
        {
        }

        internal ProvisionFlightNumber(long id, long ancillaryProvisionId, string flightNumber)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(flightNumber);
        }

        public long AncillaryProvisionId { get; private set; }

        public string FlightNumber { get; private set; } = default!;

        internal void Change(string flightNumber)
        {
            var normalized = (flightNumber ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionFlightNumber)}.{nameof(FlightNumber)}");

            FlightNumber = normalized;
        }

        internal bool SameAs(ProvisionFlightNumber other) => string.Equals(FlightNumber, other.FlightNumber, StringComparison.Ordinal);
    }
}
