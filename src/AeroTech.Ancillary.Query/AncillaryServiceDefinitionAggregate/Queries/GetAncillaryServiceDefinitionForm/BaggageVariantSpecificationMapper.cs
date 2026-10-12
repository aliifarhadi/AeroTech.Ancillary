using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class BaggageVariantSpecificationMapper
    {
        public static BaggageVariantSpecificationDto ToDto(AncillaryServiceDefinition definition)
        {
            var baggage = definition.Baggage!.ToArgs();

            return definition.VariantCode switch
            {
                AncillaryVariant.ExtraCheckedBag => new ExtraCheckedBagSpecificationDto(baggage.MaxKgPerPiece),
                AncillaryVariant.ExtraWeightPackage => new ExtraWeightPackageSpecificationDto(baggage.PackageWeightKg!.Value),
                AncillaryVariant.Overweight => new OverweightBagSpecificationDto(
                    baggage.AllowanceConcept,
                    baggage.MaxKgPerPiece,
                    baggage.WeightFromExclusiveKg!.Value,
                    baggage.WeightToInclusiveKg!.Value,
                    baggage.ChargeCombination),
                AncillaryVariant.Oversize => new OversizeBagSpecificationDto(
                    baggage.AllowanceConcept,
                    baggage.MaxKgPerPiece,
                    ToDto(baggage.MaxSize),
                    baggage.MaxLinearSumCm,
                    baggage.ChargeCombination),
                AncillaryVariant.CabinBag => new CabinBagSpecificationDto(baggage.AllowanceConcept!.Value, baggage.MaxKgPerPiece, ToDto(baggage.MaxSize)),
                _ => new SpecialEquipmentSpecificationDto(
                    baggage.AllowanceConcept,
                    baggage.MaxKgPerPiece,
                    ToDto(baggage.MaxSize),
                    baggage.MaxLinearSumCm,
                    baggage.EquipmentKind!)
            };
        }

        private static DimensionsCmDto? ToDto(DimensionsCmArgs? size)
            => size is null ? null : new DimensionsCmDto(size.LengthCm, size.WidthCm, size.HeightCm);
    }
}
