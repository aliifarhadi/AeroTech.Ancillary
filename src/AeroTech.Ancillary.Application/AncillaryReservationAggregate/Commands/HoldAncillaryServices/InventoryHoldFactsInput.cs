using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record InventoryHoldFactsInput(DateTimeOffset? TimeWithOffset, int? GuestCount);
}
