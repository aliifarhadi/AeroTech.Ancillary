using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(FreeSpecialMealSpecificationDto))]
    [JsonDerivedType(typeof(PaidPreorderMealSpecificationDto))]
    public abstract record MealVariantSpecificationDto;

    [VariantCode(AncillaryVariant.FreeSpecialMeal)]
    public sealed record FreeSpecialMealSpecificationDto(
        string MealCode,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode) : MealVariantSpecificationDto;

    [VariantCode(AncillaryVariant.PaidPreorderMeal)]
    public sealed record PaidPreorderMealSpecificationDto(
        string? MealCode,
        string MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode) : MealVariantSpecificationDto;
}
