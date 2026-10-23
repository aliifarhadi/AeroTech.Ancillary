using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficePriorityDetailsDto(
        EnumValueDto Kind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PriorityZoneCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? PriorityGroupCode,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);
}
