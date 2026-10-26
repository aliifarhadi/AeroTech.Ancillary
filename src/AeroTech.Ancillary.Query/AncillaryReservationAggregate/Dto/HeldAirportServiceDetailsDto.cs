using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldAirportServiceDetailsDto(
        int AirportId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? FacilityId);
}
