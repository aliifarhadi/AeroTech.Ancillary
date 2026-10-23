using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeMealDetailsDto(
        EnumValueDto MealKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MealCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MenuItemRef,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? DietaryCode,
        int CateringLeadTimeMinutes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ExclusiveMealFamilyCode);
}
