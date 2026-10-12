using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(ExtraCheckedBagSpecificationDto))]
    [JsonDerivedType(typeof(ExtraWeightPackageSpecificationDto))]
    [JsonDerivedType(typeof(OverweightBagSpecificationDto))]
    [JsonDerivedType(typeof(OversizeBagSpecificationDto))]
    [JsonDerivedType(typeof(CabinBagSpecificationDto))]
    [JsonDerivedType(typeof(SpecialEquipmentSpecificationDto))]
    public abstract record BaggageVariantSpecificationDto;

    [VariantCode(AncillaryVariant.ExtraCheckedBag)]
    public sealed record ExtraCheckedBagSpecificationDto(
        decimal? MaxKgPerPiece) : BaggageVariantSpecificationDto;

    [VariantCode(AncillaryVariant.ExtraWeightPackage)]
    public sealed record ExtraWeightPackageSpecificationDto(
        decimal PackageWeightKg) : BaggageVariantSpecificationDto;

    [VariantCode(AncillaryVariant.Overweight)]
    public sealed record OverweightBagSpecificationDto(
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? MaxKgPerPiece,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg,
        BaggageChargeCombination? ChargeCombination) : BaggageVariantSpecificationDto;

    [VariantCode(AncillaryVariant.Oversize)]
    public sealed record OversizeBagSpecificationDto(
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? MaxKgPerPiece,
        DimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        BaggageChargeCombination? ChargeCombination) : BaggageVariantSpecificationDto;

    [VariantCode(AncillaryVariant.CabinBag)]
    public sealed record CabinBagSpecificationDto(
        BaggageAllowanceConcept AllowanceConcept,
        decimal? MaxKgPerPiece,
        DimensionsCmDto? MaxSize) : BaggageVariantSpecificationDto;

    [VariantCode(AncillaryVariant.SpecialEquipment)]
    public sealed record SpecialEquipmentSpecificationDto(
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? MaxKgPerPiece,
        DimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        string EquipmentKind) : BaggageVariantSpecificationDto;
}
