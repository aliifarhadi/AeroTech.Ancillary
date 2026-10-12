using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(FreeSpecialMealSpecificationDto))]
    [JsonDerivedType(typeof(PaidPreorderMealSpecificationDto))]
    public abstract record MealVariantSpecificationDto;

    public sealed record FreeSpecialMealSpecificationDto(
        string MealCode,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode) : MealVariantSpecificationDto;

    public sealed record PaidPreorderMealSpecificationDto(
        string? MealCode,
        string MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode) : MealVariantSpecificationDto;
}
