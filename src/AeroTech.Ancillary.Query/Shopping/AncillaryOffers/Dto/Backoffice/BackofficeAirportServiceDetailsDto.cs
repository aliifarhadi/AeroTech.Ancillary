using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAirportServiceDetailsDto(
        EnumValueDto Kind,
        int AirportId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? TerminalRef,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? FacilityId,
        EnumValueDto Direction,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ServiceWindowStart,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ServiceWindowEnd,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? IanaTimeZone,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? VisitDurationMinutes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MaxGuestsPerPrimary,
        bool RequiresSpecificAppointment,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? IncludedComponentCodes);
}
