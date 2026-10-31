using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.RestApi.V1.SeatMaps.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PriceOfferSeatsRequest(string? OfferId, IReadOnlyList<string> Ids);
}
