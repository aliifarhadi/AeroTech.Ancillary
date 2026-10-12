using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [JsonDerivedType(typeof(LoungeSpecificationDto))]
    [JsonDerivedType(typeof(FastTrackSpecificationDto))]
    [JsonDerivedType(typeof(CipMeetAssistSpecificationDto))]
    public abstract record AirportServiceVariantSpecificationDto;

    [VariantCode(AncillaryVariant.Lounge)]
    public sealed record LoungeSpecificationDto(
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        AirportServiceDirection Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        bool RequiresSpecificAppointment) : AirportServiceVariantSpecificationDto;

    [VariantCode(AncillaryVariant.FastTrack)]
    public sealed record FastTrackSpecificationDto(
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        AirportServiceDirection Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        bool RequiresSpecificAppointment) : AirportServiceVariantSpecificationDto;

    [VariantCode(AncillaryVariant.CipMeetAssist)]
    public sealed record CipMeetAssistSpecificationDto(
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        AirportServiceDirection Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        [property: MinLength(1)] IReadOnlyList<string> IncludedComponentCodes,
        bool RequiresSpecificAppointment) : AirportServiceVariantSpecificationDto;
}
