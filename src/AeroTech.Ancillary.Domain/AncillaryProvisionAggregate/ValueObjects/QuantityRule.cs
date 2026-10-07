using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class QuantityRule : ValueObject
    {
        private QuantityRule()
        {
        }

        private QuantityRule(AncillaryQuantityUnit unit, int minQuantity, int maxQuantity)
        {
            Unit = unit;
            MinQuantity = minQuantity;
            MaxQuantity = maxQuantity;
        }

        public AncillaryQuantityUnit Unit { get; private set; }

        public int MinQuantity { get; private set; }

        public int MaxQuantity { get; private set; }

        public static QuantityRule Create(AncillaryQuantityUnit unit, int minQuantity, int maxQuantity)
        {
            Require(Enum.IsDefined(unit), nameof(Unit));
            Require(minQuantity >= 1, nameof(MinQuantity));
            Require(maxQuantity >= minQuantity, nameof(MaxQuantity));

            return new QuantityRule(unit, minQuantity, maxQuantity);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Unit;
            yield return MinQuantity;
            yield return MaxQuantity;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(QuantityRule)}.{field}");
        }
    }
}
