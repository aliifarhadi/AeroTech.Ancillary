using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record BassinetDetailsDto(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] decimal? MaxInfantWeightKg,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MaxInfantAgeMonths,
        IReadOnlyList<string> CompatibleSeatGroups,
        bool RequiresInfantAndGuardian);
}
