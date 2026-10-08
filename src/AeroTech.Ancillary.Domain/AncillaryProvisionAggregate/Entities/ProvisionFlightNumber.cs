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

        internal ProvisionFlightNumber(long id, long ancillaryProvisionId, long provisionFlightApplicationRuleId, string flightNumber)
        {
            var normalized = (flightNumber ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionFlightNumber)}.{nameof(FlightNumber)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFlightApplicationRuleId = provisionFlightApplicationRuleId;
            FlightNumber = normalized;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFlightApplicationRuleId { get; private set; }

        public string FlightNumber { get; private set; } = default!;

        internal bool SameAs(ProvisionFlightNumber other) => string.Equals(FlightNumber, other.FlightNumber, StringComparison.Ordinal);
    }
}
