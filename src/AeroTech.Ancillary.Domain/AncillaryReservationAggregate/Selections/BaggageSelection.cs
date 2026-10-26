using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class BaggageSelection
    {
        private const string Name = nameof(BaggageSelection);
        private const int BagRefMaxLength = 64;
        private const int EquipmentKindMaxLength = 20;

        private BaggageSelection()
        {
        }

        public string? BagRef { get; private set; }

        public decimal? MeasuredWeightKg { get; private set; }

        public SelectedDimensions? Dimensions { get; private set; }

        public decimal? WeightKg { get; private set; }

        public string? EquipmentKind { get; private set; }

        internal static BaggageSelection Create(BaggageSelectionArgs args)
        {
            Require(args is not { BagRef: null, MeasuredWeightKg: null, Dimensions: null, WeightKg: null, EquipmentKind: null }, Name);
            Require(SpecificationRules.IsText(args.BagRef, BagRefMaxLength), nameof(BagRef));
            Require(SpecificationRules.IsMeasure(args.MeasuredWeightKg), nameof(MeasuredWeightKg));
            Require(SpecificationRules.IsMeasure(args.WeightKg), nameof(WeightKg));
            Require(args.EquipmentKind is null || SpecificationRules.IsCode(args.EquipmentKind, EquipmentKindMaxLength), nameof(EquipmentKind));

            return new BaggageSelection
            {
                BagRef = args.BagRef,
                MeasuredWeightKg = args.MeasuredWeightKg,
                Dimensions = args.Dimensions is null ? null : SelectedDimensions.Create(args.Dimensions, Name),
                WeightKg = args.WeightKg,
                EquipmentKind = args.EquipmentKind
            };
        }

        public BaggageSelectionArgs ToArgs() => new(BagRef, MeasuredWeightKg, Dimensions?.ToArgs(), WeightKg, EquipmentKind);

        internal bool IsChosenBy(BaggageSelectionArgs? chosen)
            => BagRef == chosen?.BagRef
               && MeasuredWeightKg == chosen?.MeasuredWeightKg
               && Dimensions?.ToArgs() == chosen?.Dimensions
               && WeightKg == chosen?.WeightKg;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{Name}.{field}");
        }
    }
}
