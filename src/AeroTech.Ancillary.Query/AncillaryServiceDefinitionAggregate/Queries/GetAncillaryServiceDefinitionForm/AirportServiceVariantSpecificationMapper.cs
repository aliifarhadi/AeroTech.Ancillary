using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class AirportServiceVariantSpecificationMapper
    {
        public static AirportServiceVariantSpecificationDto ToDto(AncillaryServiceDefinition definition)
        {
            var service = definition.AirportService!.ToArgs();

            return definition.VariantCode switch
            {
                AncillaryVariant.Lounge => new LoungeSpecificationDto(
                    service.AirportId,
                    service.TerminalRef,
                    service.FacilityId,
                    service.Direction,
                    service.ServiceWindowStart,
                    service.ServiceWindowEnd,
                    service.IanaTimeZone,
                    service.VisitDurationMinutes,
                    service.MaxGuestsPerPrimary,
                    service.RequiresSpecificAppointment),
                AncillaryVariant.FastTrack => new FastTrackSpecificationDto(
                    service.AirportId,
                    service.TerminalRef,
                    service.FacilityId,
                    service.Direction,
                    service.ServiceWindowStart,
                    service.ServiceWindowEnd,
                    service.IanaTimeZone,
                    service.VisitDurationMinutes,
                    service.MaxGuestsPerPrimary,
                    service.RequiresSpecificAppointment),
                _ => new CipMeetAssistSpecificationDto(
                    service.AirportId,
                    service.TerminalRef,
                    service.FacilityId,
                    service.Direction,
                    service.ServiceWindowStart,
                    service.ServiceWindowEnd,
                    service.IanaTimeZone,
                    service.VisitDurationMinutes,
                    service.MaxGuestsPerPrimary,
                    service.IncludedComponentCodes,
                    service.RequiresSpecificAppointment)
            };
        }
    }
}
