using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionSeatCharacteristic : Entity<long>
    {
        private const int MaxLength = 25;

        private ProvisionSeatCharacteristic()
        {
        }

        internal ProvisionSeatCharacteristic(long id, long ancillaryProvisionId, long provisionSeatApplicationRuleId, string characteristicCode)
        {
            var normalized = (characteristicCode ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionSeatCharacteristic)}.{nameof(CharacteristicCode)}");

            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            ProvisionSeatApplicationRuleId = provisionSeatApplicationRuleId;
            CharacteristicCode = normalized;
        }

        public long AncillaryProvisionId { get; private set; }

        public long ProvisionSeatApplicationRuleId { get; private set; }

        public string CharacteristicCode { get; private set; } = default!;

        internal bool SameAs(ProvisionSeatCharacteristic other) => string.Equals(CharacteristicCode, other.CharacteristicCode, StringComparison.Ordinal);
    }
}
