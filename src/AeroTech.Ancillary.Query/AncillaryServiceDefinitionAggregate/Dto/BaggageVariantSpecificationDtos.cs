using System.Text.Json.Serialization;
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

    public sealed record ExtraCheckedBagSpecificationDto(
        decimal? MaxKgPerPiece) : BaggageVariantSpecificationDto;

    public sealed record ExtraWeightPackageSpecificationDto(
        decimal PackageWeightKg) : BaggageVariantSpecificationDto;

    public sealed record OverweightBagSpecificationDto(
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? MaxKgPerPiece,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg,
        BaggageChargeCombination? ChargeCombination) : BaggageVariantSpecificationDto;

    public sealed record OversizeBagSpecificationDto(
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? MaxKgPerPiece,
        DimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        BaggageChargeCombination? ChargeCombination) : BaggageVariantSpecificationDto;

    public sealed record CabinBagSpecificationDto(
        BaggageAllowanceConcept AllowanceConcept,
        decimal? MaxKgPerPiece,
        DimensionsCmDto? MaxSize) : BaggageVariantSpecificationDto;

    public sealed record SpecialEquipmentSpecificationDto(
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? MaxKgPerPiece,
        DimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        string EquipmentKind) : BaggageVariantSpecificationDto;
}
