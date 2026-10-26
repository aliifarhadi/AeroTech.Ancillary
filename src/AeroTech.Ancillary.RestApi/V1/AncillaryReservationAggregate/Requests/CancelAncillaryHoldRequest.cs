using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record CancelAncillaryHoldRequest(string ReasonCode, IReadOnlyList<long>? UnitIds);
}
