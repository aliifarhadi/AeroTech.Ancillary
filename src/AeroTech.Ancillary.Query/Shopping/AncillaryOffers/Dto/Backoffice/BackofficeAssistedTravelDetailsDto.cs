using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAssistedTravelDetailsDto(
        EnumValueDto AssistanceKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] WheelchairDetailsDto? Wheelchair,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BackofficeDisabilityAssistanceDetailsDto? DisabilityAssistance,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BassinetDetailsDto? Bassinet);
}
