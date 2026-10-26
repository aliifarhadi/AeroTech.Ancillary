using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record HeldAirportServiceOptionsDto(HeldAirportServiceSelectionDto AirportService);

    public sealed record HeldAirportServiceSelectionDto(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? TimeWithOffset,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? GuestCount);
}
