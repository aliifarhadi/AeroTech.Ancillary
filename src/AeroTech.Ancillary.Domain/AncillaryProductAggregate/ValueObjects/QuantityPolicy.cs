using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects
{
    public sealed class QuantityPolicy : ValueObject
    {
        private const int MaxLimit = 99;

        private QuantityPolicy()
        {
        }

        public QuantityPolicy(AncillaryQuantityUnit unit, int min, int max)
        {
            if (!Enum.IsDefined(unit))
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(QuantityPolicy)}.{nameof(Unit)}");

            if (min < 1)
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(QuantityPolicy)}.{nameof(Min)}");

            if (max < min || max > MaxLimit)
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(QuantityPolicy)}.{nameof(Max)}");

            Unit = unit;
            Min = min;
            Max = max;
        }

        public AncillaryQuantityUnit Unit { get; private set; }

        public int Min { get; private set; }

        public int Max { get; private set; }

        public QuantityPolicy Copy() => new(Unit, Min, Max);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Unit;
            yield return Min;
            yield return Max;
        }
    }
}
