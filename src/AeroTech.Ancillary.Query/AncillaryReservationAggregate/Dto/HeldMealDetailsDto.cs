using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldMealDetailsDto(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MealCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? MenuItemRef);
}
