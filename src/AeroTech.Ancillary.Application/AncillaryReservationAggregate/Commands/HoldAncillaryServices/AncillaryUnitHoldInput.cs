using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AncillaryUnitHoldInput<TSelected>(
        long OrderServiceId,
        HoldUnitInput? Unit = null,
        HoldCoverageInput? Coverage = null,
        TSelected? Selected = null)
        where TSelected : class;
}
