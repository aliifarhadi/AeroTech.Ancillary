using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.RestApi.V1.SeatMaps.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PriceOrderSeatsRequest(long? OrderId, IReadOnlyList<string> Ids);
}
