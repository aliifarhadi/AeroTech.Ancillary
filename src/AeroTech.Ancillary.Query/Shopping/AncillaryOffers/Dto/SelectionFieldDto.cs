using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record SelectionFieldDto(
        string Control,
        bool Required,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<SelectionFieldChoiceDto>? Choices,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Unit,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Min,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? Max);
}
