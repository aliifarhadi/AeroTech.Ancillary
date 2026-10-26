using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AncillaryProductHoldInput<TSelected>(long ProvisionId, IReadOnlyList<AncillaryUnitHoldInput<TSelected>> Units)
        where TSelected : class;
}
