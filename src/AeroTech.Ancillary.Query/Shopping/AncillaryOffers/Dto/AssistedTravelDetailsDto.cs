using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record AssistedTravelDetailsDto(
        AssistanceKind AssistanceKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] WheelchairDetailsDto? Wheelchair,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DisabilityAssistanceDetailsDto? DisabilityAssistance,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] BassinetDetailsDto? Bassinet);
}
