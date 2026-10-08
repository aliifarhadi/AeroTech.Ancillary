using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionSeatNumber : Entity<long>
    {
        private const int MaxLength = 16;

        private ProvisionSeatNumber()
        {
        }

        internal ProvisionSeatNumber(long id, long ancillaryProvisionId, long provisionSeatApplicationRuleId, string seatNumber)
        {
            var normalized = (seatNumber ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionSeatNumber)}.{nameof(SeatNumber)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionSeatApplicationRuleId = provisionSeatApplicationRuleId;
            SeatNumber = normalized;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionSeatApplicationRuleId { get; private set; }

        public string SeatNumber { get; private set; } = default!;

        internal bool SameAs(ProvisionSeatNumber other) => string.Equals(SeatNumber, other.SeatNumber, StringComparison.Ordinal);
    }
}
