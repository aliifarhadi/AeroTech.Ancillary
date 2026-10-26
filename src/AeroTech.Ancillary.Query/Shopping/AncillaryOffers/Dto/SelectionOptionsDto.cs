using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record SelectionOptionsDto<TValue>(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TValue? Baggage = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TValue? Pet = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TValue? AssistedTravel = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TValue? AirportService = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TValue? Priority = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TValue? Connectivity = null)
        where TValue : class;
}
