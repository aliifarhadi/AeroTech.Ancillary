using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class MealSelection
    {
        private const string Name = nameof(MealSelection);
        private const int MealCodeLength = 4;
        private const int MenuItemRefMaxLength = 40;

        private MealSelection()
        {
        }

        public string? MealCode { get; private set; }

        public string? MenuItemRef { get; private set; }

        internal static MealSelection Create(MealSelectionArgs args)
        {
            Require(args is not { MealCode: null, MenuItemRef: null }, Name);
            Require(args.MealCode is null || (args.MealCode.Length == MealCodeLength && SpecificationRules.IsCode(args.MealCode, MealCodeLength)), nameof(MealCode));
            Require(args.MenuItemRef is null || SpecificationRules.IsCode(args.MenuItemRef, MenuItemRefMaxLength), nameof(MenuItemRef));

            return new MealSelection
            {
                MealCode = args.MealCode,
                MenuItemRef = args.MenuItemRef
            };
        }

        public MealSelectionArgs ToArgs() => new(MealCode, MenuItemRef);

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{Name}.{field}");
        }
    }
}
