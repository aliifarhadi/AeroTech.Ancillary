using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1._Shared.OpenApi;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    [VariantSpecification(
        typeof(AirportServiceVariantSpecificationDto),
        "The airport service fields of the variant named by variantCode: A20 Lounge, A21 Fast track, A22 CIP meet and assist. The first v1 " +
        "body is still accepted: kind and includedComponentCodes may be sent on every variant and must then agree with it, otherwise the " +
        "answer is 422 with code 16202.")]
    public sealed record AirportServiceSpecificationRequest(
        AirportServiceKind? Kind,
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        AirportServiceDirection Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        IReadOnlyList<string>? IncludedComponentCodes,
        bool RequiresSpecificAppointment)
    {
        public AirportServiceSpecificationInput ToInput(string variantCode)
            => new(
                Kind ?? variantCode switch
                {
                    AncillaryVariant.FastTrack => AirportServiceKind.FastTrack,
                    AncillaryVariant.CipMeetAssist => AirportServiceKind.Cip,
                    _ => AirportServiceKind.Lounge
                },
                AirportId,
                TerminalRef,
                FacilityId,
                Direction,
                ServiceWindowStart,
                ServiceWindowEnd,
                IanaTimeZone,
                VisitDurationMinutes,
                MaxGuestsPerPrimary,
                IncludedComponentCodes,
                RequiresSpecificAppointment);
    }
}
