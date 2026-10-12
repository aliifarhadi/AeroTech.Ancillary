using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class MealVariantSpecificationMapper
    {
        public static MealVariantSpecificationDto ToDto(AncillaryServiceDefinition definition)
        {
            var meal = definition.Meal!.ToArgs();

            return definition.VariantCode switch
            {
                AncillaryVariant.FreeSpecialMeal => new FreeSpecialMealSpecificationDto(
                    meal.MealCode!,
                    meal.DietaryCode,
                    meal.CateringLeadTimeMinutes,
                    meal.ExclusiveMealFamilyCode),
                _ => new PaidPreorderMealSpecificationDto(
                    meal.MealCode,
                    meal.MenuItemRef!,
                    meal.DietaryCode,
                    meal.CateringLeadTimeMinutes,
                    meal.ExclusiveMealFamilyCode)
            };
        }
    }
}
