using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record MealDetailsDto(
        MealKind MealKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MealCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MenuItemRef,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? DietaryCode,
        int CateringLeadTimeMinutes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ExclusiveMealFamilyCode);
}
