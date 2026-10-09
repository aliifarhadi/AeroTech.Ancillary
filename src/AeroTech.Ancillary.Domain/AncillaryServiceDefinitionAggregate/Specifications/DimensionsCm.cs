using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class DimensionsCm : ValueObject
    {
        private DimensionsCm()
        {
        }

        private DimensionsCm(decimal lengthCm, decimal widthCm, decimal heightCm)
        {
            LengthCm = lengthCm;
            WidthCm = widthCm;
            HeightCm = heightCm;
        }

        public decimal LengthCm { get; private set; }

        public decimal WidthCm { get; private set; }

        public decimal HeightCm { get; private set; }

        internal static DimensionsCm Create(DimensionsCmArgs args, string specification, string field)
        {
            SpecificationRules.Require(
                SpecificationRules.IsMeasure(args.LengthCm) && SpecificationRules.IsMeasure(args.WidthCm) && SpecificationRules.IsMeasure(args.HeightCm),
                specification,
                field);

            return new DimensionsCm(args.LengthCm, args.WidthCm, args.HeightCm);
        }

        public DimensionsCmArgs ToArgs() => new(LengthCm, WidthCm, HeightCm);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return LengthCm;
            yield return WidthCm;
            yield return HeightCm;
        }
    }
}
