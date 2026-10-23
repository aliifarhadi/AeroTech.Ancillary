using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record WheelchairDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? AssistanceLevelCode,
        int LeadTimeMinutes);
}
