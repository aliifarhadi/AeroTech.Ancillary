using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects
{
    public sealed class BaggageDetail : ValueObject
    {
        private const int WeightScale = 2;

        private BaggageDetail()
        {
        }

        public BaggageDetail(int? pieces, decimal? weight, AncillaryWeightUnit? weightUnit)
        {
            if (pieces is null && weight is null)
                throw ExceptionFactory.AncillaryProductIsInvalid(nameof(BaggageDetail));

            if (pieces is < 1)
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(BaggageDetail)}.{nameof(Pieces)}");

            if (weight is { } value && (value <= 0 || decimal.Round(value, WeightScale) != value))
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(BaggageDetail)}.{nameof(Weight)}");

            if (weight.HasValue != weightUnit.HasValue || (weightUnit is { } unit && !Enum.IsDefined(unit)))
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(BaggageDetail)}.{nameof(WeightUnit)}");

            Pieces = pieces;
            Weight = weight;
            WeightUnit = weightUnit;
        }

        public int? Pieces { get; private set; }

        public decimal? Weight { get; private set; }

        public AncillaryWeightUnit? WeightUnit { get; private set; }

        public BaggageDetail Copy() => new(Pieces, Weight, WeightUnit);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Pieces;
            yield return Weight;
            yield return WeightUnit;
        }
    }
}
