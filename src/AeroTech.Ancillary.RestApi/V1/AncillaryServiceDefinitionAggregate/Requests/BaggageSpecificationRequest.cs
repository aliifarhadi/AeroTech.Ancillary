using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1._Shared.OpenApi;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    [VariantSpecification(
        typeof(BaggageVariantSpecificationDto),
        "The baggage fields of the variant named by variantCode: A01 Extra checked bag, A02 Extra weight package, A03 Overweight bag, " +
        "A04 Oversize bag, A05 Cabin bag, A06 Sports and special equipment. The first v1 body is still accepted: chargeKind and every other " +
        "baggage member may be sent on every variant and must then agree with it, otherwise the answer is 422 with code 16202.")]
    public sealed record BaggageSpecificationRequest(
        BaggageChargeKind? ChargeKind,
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? PackageWeightKg,
        decimal? MaxKgPerPiece,
        decimal? WeightFromExclusiveKg,
        decimal? WeightToInclusiveKg,
        DimensionsCmInput? MaxSize,
        decimal? MaxLinearSumCm,
        string? EquipmentKind,
        BaggageChargeCombination? ChargeCombination)
    {
        public BaggageSpecificationInput ToInput(string variantCode)
            => new(
                ChargeKind ?? variantCode switch
                {
                    AncillaryVariant.ExtraWeightPackage => BaggageChargeKind.WeightPackage,
                    AncillaryVariant.Overweight => BaggageChargeKind.Overweight,
                    AncillaryVariant.Oversize => BaggageChargeKind.Oversize,
                    AncillaryVariant.SpecialEquipment => BaggageChargeKind.SpecialEquipment,
                    _ => BaggageChargeKind.ExtraPiece
                },
                AllowanceConcept ?? variantCode switch
                {
                    AncillaryVariant.ExtraCheckedBag => BaggageAllowanceConcept.Piece,
                    AncillaryVariant.ExtraWeightPackage => BaggageAllowanceConcept.Weight,
                    _ => (BaggageAllowanceConcept?)null
                },
                PackageWeightKg,
                MaxKgPerPiece,
                WeightFromExclusiveKg,
                WeightToInclusiveKg,
                MaxSize,
                MaxLinearSumCm,
                EquipmentKind,
                ChargeCombination);
    }
}
