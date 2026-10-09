using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class BaggageSpecification
    {
        private const string Name = nameof(BaggageSpecification);
        private const int EquipmentKindMaxLength = 20;

        private static readonly Dictionary<string, BaggageChargeKind> ChargeKinds = new(StringComparer.Ordinal)
        {
            [AncillaryVariant.ExtraCheckedBag] = BaggageChargeKind.ExtraPiece,
            [AncillaryVariant.ExtraWeightPackage] = BaggageChargeKind.WeightPackage,
            [AncillaryVariant.Overweight] = BaggageChargeKind.Overweight,
            [AncillaryVariant.Oversize] = BaggageChargeKind.Oversize,
            [AncillaryVariant.CabinBag] = BaggageChargeKind.ExtraPiece,
            [AncillaryVariant.SpecialEquipment] = BaggageChargeKind.SpecialEquipment
        };

        private BaggageSpecification()
        {
        }

        public BaggageChargeKind ChargeKind { get; private set; }

        public BaggageAllowanceConcept? AllowanceConcept { get; private set; }

        public decimal? PackageWeightKg { get; private set; }

        public decimal? MaxKgPerPiece { get; private set; }

        public decimal? WeightFromExclusiveKg { get; private set; }

        public decimal? WeightToInclusiveKg { get; private set; }

        public DimensionsCm? MaxSize { get; private set; }

        public decimal? MaxLinearSumCm { get; private set; }

        public string? EquipmentKind { get; private set; }

        public BaggageChargeCombination? ChargeCombination { get; private set; }

        internal static BaggageSpecification Create(AncillaryVariant variant, BaggageSpecificationArgs args)
        {
            var code = variant.Code;
            var isPackage = code == AncillaryVariant.ExtraWeightPackage;
            var isOverweight = code == AncillaryVariant.Overweight;
            var isOversize = code == AncillaryVariant.Oversize;
            var isEquipment = code == AncillaryVariant.SpecialEquipment;
            var needsConcept = code is AncillaryVariant.ExtraCheckedBag or AncillaryVariant.ExtraWeightPackage or AncillaryVariant.CabinBag;
            var sized = isOversize || isEquipment || code == AncillaryVariant.CabinBag;

            Require(args.ChargeKind == ChargeKinds[code], nameof(ChargeKind));
            Require(args.AllowanceConcept is null || Enum.IsDefined(args.AllowanceConcept.Value), nameof(AllowanceConcept));
            Require(!needsConcept || args.AllowanceConcept is not null, nameof(AllowanceConcept));
            Require(code != AncillaryVariant.ExtraCheckedBag || args.AllowanceConcept == BaggageAllowanceConcept.Piece, nameof(AllowanceConcept));
            Require(!isPackage || args.AllowanceConcept == BaggageAllowanceConcept.Weight, nameof(AllowanceConcept));
            Require(isPackage == args.PackageWeightKg is not null && SpecificationRules.IsMeasure(args.PackageWeightKg), nameof(PackageWeightKg));
            Require((!isPackage || args.MaxKgPerPiece is null) && SpecificationRules.IsMeasure(args.MaxKgPerPiece), nameof(MaxKgPerPiece));
            Require(
                isOverweight
                    ? args.WeightFromExclusiveKg is >= 0 && args.WeightToInclusiveKg > args.WeightFromExclusiveKg && SpecificationRules.IsMeasure(args.WeightToInclusiveKg)
                    : args.WeightFromExclusiveKg is null && args.WeightToInclusiveKg is null,
                nameof(WeightToInclusiveKg));
            Require(sized || args.MaxSize is null, nameof(MaxSize));
            Require((isOversize || isEquipment || args.MaxLinearSumCm is null) && SpecificationRules.IsMeasure(args.MaxLinearSumCm), nameof(MaxLinearSumCm));
            Require(!isOversize || args.MaxSize is not null || args.MaxLinearSumCm is not null, nameof(MaxSize));
            Require(
                isEquipment ? SpecificationRules.IsCode(args.EquipmentKind, EquipmentKindMaxLength) : args.EquipmentKind is null,
                nameof(EquipmentKind));
            Require(
                args.ChargeCombination is null || ((isOverweight || isOversize) && Enum.IsDefined(args.ChargeCombination.Value)),
                nameof(ChargeCombination));

            return new BaggageSpecification
            {
                ChargeKind = args.ChargeKind,
                AllowanceConcept = args.AllowanceConcept,
                PackageWeightKg = args.PackageWeightKg,
                MaxKgPerPiece = args.MaxKgPerPiece,
                WeightFromExclusiveKg = args.WeightFromExclusiveKg,
                WeightToInclusiveKg = args.WeightToInclusiveKg,
                MaxSize = args.MaxSize is null ? null : DimensionsCm.Create(args.MaxSize, Name, nameof(MaxSize)),
                MaxLinearSumCm = args.MaxLinearSumCm,
                EquipmentKind = args.EquipmentKind,
                ChargeCombination = args.ChargeCombination
            };
        }

        public BaggageSpecificationArgs ToArgs()
            => new(
                ChargeKind,
                AllowanceConcept,
                PackageWeightKg,
                MaxKgPerPiece,
                WeightFromExclusiveKg,
                WeightToInclusiveKg,
                MaxSize?.ToArgs(),
                MaxLinearSumCm,
                EquipmentKind,
                ChargeCombination);

        private static void Require(bool condition, string field) => SpecificationRules.Require(condition, Name, field);
    }
}
