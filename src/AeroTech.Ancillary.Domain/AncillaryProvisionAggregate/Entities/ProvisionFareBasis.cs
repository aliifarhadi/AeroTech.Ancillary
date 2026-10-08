using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFareBasis : Entity<long>
    {
        private const int MaxLength = 64;

        private ProvisionFareBasis()
        {
        }

        internal ProvisionFareBasis(long id, long ancillaryProvisionId, long provisionFareApplicationRuleId, string fareBasisCode)
        {
            var normalized = (fareBasisCode ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionFareBasis)}.{nameof(FareBasisCode)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionFareApplicationRuleId = provisionFareApplicationRuleId;
            FareBasisCode = normalized;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionFareApplicationRuleId { get; private set; }

        public string FareBasisCode { get; private set; } = default!;

        internal bool SameAs(ProvisionFareBasis other) => string.Equals(FareBasisCode, other.FareBasisCode, StringComparison.Ordinal);
    }
}
