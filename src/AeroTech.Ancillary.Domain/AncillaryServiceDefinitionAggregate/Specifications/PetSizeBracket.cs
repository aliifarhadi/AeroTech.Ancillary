using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class PetSizeBracket
    {
        private const int CodeMaxLength = 20;

        private PetSizeBracket()
        {
        }

        public string Code { get; private set; } = default!;

        public decimal WeightFromExclusiveKg { get; private set; }

        public decimal WeightToInclusiveKg { get; private set; }

        internal static PetSizeBracket Create(PetSizeBracketArgs args)
        {
            SpecificationRules.Require(args is not null && SpecificationRules.IsCode(args.Code, CodeMaxLength), nameof(PetSpecification), nameof(Code));
            SpecificationRules.Require(
                args!.WeightFromExclusiveKg >= 0
                && args.WeightToInclusiveKg > args.WeightFromExclusiveKg
                && SpecificationRules.IsMeasure(args.WeightToInclusiveKg)
                && args.WeightFromExclusiveKg == decimal.Round(args.WeightFromExclusiveKg, 3),
                nameof(PetSpecification),
                nameof(WeightToInclusiveKg));

            return new PetSizeBracket
            {
                Code = args.Code,
                WeightFromExclusiveKg = args.WeightFromExclusiveKg,
                WeightToInclusiveKg = args.WeightToInclusiveKg
            };
        }
    }
}
