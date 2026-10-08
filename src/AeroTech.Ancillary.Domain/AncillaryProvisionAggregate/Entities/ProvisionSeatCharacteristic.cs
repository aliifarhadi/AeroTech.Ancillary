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

        internal ProvisionSeatCharacteristic(long id, long ancillaryProvisionId, string characteristicCode)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            Change(characteristicCode);
        }

        public long AncillaryProvisionId { get; private set; }

        public string CharacteristicCode { get; private set; } = default!;

        internal void Change(string characteristicCode)
        {
            var normalized = (characteristicCode ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized.Length is < 1 or > MaxLength || normalized.Any(char.IsWhiteSpace))
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionSeatCharacteristic)}.{nameof(CharacteristicCode)}");

            CharacteristicCode = normalized;
        }

        internal bool SameAs(ProvisionSeatCharacteristic other) => string.Equals(CharacteristicCode, other.CharacteristicCode, StringComparison.Ordinal);
    }
}
