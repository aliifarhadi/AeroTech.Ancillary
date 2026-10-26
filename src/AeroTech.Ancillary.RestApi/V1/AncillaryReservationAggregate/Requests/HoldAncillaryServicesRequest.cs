using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record HoldAncillaryServicesRequest(
        string IdempotencyKey,
        long OrderId,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        IReadOnlyList<AncillaryHoldServiceInput> Services);
}
