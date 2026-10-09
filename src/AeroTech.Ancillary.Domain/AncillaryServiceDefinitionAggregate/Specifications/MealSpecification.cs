using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class MealSpecification
    {
        private const string Name = nameof(MealSpecification);
        private const int MealCodeLength = 4;
        private const int MenuItemRefMaxLength = 40;
        private const int DietaryCodeMaxLength = 20;
        private const int FamilyCodeMaxLength = 30;

        private MealSpecification()
        {
        }

        public MealKind MealKind { get; private set; }

        public string? MealCode { get; private set; }

        public string? MenuItemRef { get; private set; }

        public string? DietaryCode { get; private set; }

        public int CateringLeadTimeMinutes { get; private set; }

        public string? ExclusiveMealFamilyCode { get; private set; }

        internal static MealSpecification Create(AncillaryVariant variant, MealSpecificationArgs args)
        {
            var isSpecialRequest = variant.Code == AncillaryVariant.FreeSpecialMeal;

            Require(args.MealKind == (isSpecialRequest ? MealKind.SpecialRequest : MealKind.PaidPreorder), nameof(MealKind));
            Require(
                isSpecialRequest
                    ? args.MealCode is { Length: MealCodeLength } && SpecificationRules.IsCode(args.MealCode, MealCodeLength)
                    : args.MealCode is null || (args.MealCode.Length == MealCodeLength && SpecificationRules.IsCode(args.MealCode, MealCodeLength)),
                nameof(MealCode));
            Require(
                isSpecialRequest ? args.MenuItemRef is null : SpecificationRules.IsCode(args.MenuItemRef, MenuItemRefMaxLength),
                nameof(MenuItemRef));
            Require(args.DietaryCode is null || SpecificationRules.IsCode(args.DietaryCode, DietaryCodeMaxLength), nameof(DietaryCode));
            Require(args.CateringLeadTimeMinutes >= 0, nameof(CateringLeadTimeMinutes));
            Require(
                args.ExclusiveMealFamilyCode is null || SpecificationRules.IsCode(args.ExclusiveMealFamilyCode, FamilyCodeMaxLength),
                nameof(ExclusiveMealFamilyCode));

            return new MealSpecification
            {
                MealKind = args.MealKind,
                MealCode = args.MealCode,
                MenuItemRef = args.MenuItemRef,
                DietaryCode = args.DietaryCode,
                CateringLeadTimeMinutes = args.CateringLeadTimeMinutes,
                ExclusiveMealFamilyCode = args.ExclusiveMealFamilyCode
            };
        }

        public MealSpecificationArgs ToArgs()
            => new(MealKind, MealCode, MenuItemRef, DietaryCode, CateringLeadTimeMinutes, ExclusiveMealFamilyCode);

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
