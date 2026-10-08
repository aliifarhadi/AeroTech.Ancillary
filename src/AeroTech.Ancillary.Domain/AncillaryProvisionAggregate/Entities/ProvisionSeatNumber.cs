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

        internal ProvisionSeatNumber(long id, long ancillaryProvisionId, string seatNumber)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(seatNumber);
        }

        public long AncillaryProvisionId { get; private set; }

        public string SeatNumber { get; private set; } = default!;

        internal void Change(string seatNumber)
        {
            var normalized = (seatNumber ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionSeatNumber)}.{nameof(SeatNumber)}");

            SeatNumber = normalized;
        }

        internal bool SameAs(ProvisionSeatNumber other) => string.Equals(SeatNumber, other.SeatNumber, StringComparison.Ordinal);
    }
}
