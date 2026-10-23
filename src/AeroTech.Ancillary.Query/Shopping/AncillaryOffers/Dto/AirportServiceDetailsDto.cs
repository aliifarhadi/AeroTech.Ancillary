using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AirportServiceDetailsDto(
        AirportServiceKind Kind,
        int AirportId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? TerminalRef,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? FacilityId,
        AirportServiceDirection Direction,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ServiceWindowStart,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? ServiceWindowEnd,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? IanaTimeZone,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? VisitDurationMinutes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MaxGuestsPerPrimary,
        bool RequiresSpecificAppointment,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<string>? IncludedComponentCodes);
}
