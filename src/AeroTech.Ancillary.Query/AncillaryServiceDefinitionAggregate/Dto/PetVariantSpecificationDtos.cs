using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(PetInCabinSpecificationDto))]
    [JsonDerivedType(typeof(PetInHoldSpecificationDto))]
    public abstract record PetVariantSpecificationDto;

    public sealed record PetAnimalFormDto(
        PetAnimalType AnimalType,
        string? OtherCode);

    [VariantCode(AncillaryVariant.PetInCabin)]
    public sealed record PetInCabinSpecificationDto(
        [property: MinLength(1)] IReadOnlyList<PetAnimalFormDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        DimensionsCmDto CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes) : PetVariantSpecificationDto;

    [VariantCode(AncillaryVariant.PetInHold)]
    public sealed record PetInHoldSpecificationDto(
        [property: MinLength(1)] IReadOnlyList<PetAnimalFormDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        DimensionsCmDto CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        IReadOnlyList<PetSizeBracketDto> AllowedHoldAnimalSizeBrackets) : PetVariantSpecificationDto;
}
