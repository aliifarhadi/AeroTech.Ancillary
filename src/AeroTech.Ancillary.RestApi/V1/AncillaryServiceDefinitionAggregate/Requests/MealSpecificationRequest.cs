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
        typeof(MealVariantSpecificationDto),
        "The meal fields of the variant named by variantCode: A11 Free special meal, A12 Paid pre-order meal. The first v1 body is still " +
        "accepted: mealKind and menuItemRef may be sent on every variant and must then agree with it, otherwise the answer is 422 with code 16202.")]
    public sealed record MealSpecificationRequest(
        MealKind? MealKind,
        string? MealCode,
        string? MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode)
    {
        public MealSpecificationInput ToInput(string variantCode)
            => new(
                MealKind ?? variantCode switch
                {
                    AncillaryVariant.PaidPreorderMeal => Messages.Ancillary.Enums.MealKind.PaidPreorder,
                    _ => Messages.Ancillary.Enums.MealKind.SpecialRequest
                },
                MealCode,
                MenuItemRef,
                DietaryCode,
                CateringLeadTimeMinutes,
                ExclusiveMealFamilyCode);
    }
}
