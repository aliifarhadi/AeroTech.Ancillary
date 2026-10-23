using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record PriorityDetailsDto(
        PriorityKind Kind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PriorityZoneCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PriorityGroupCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);
}
