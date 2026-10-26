using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AncillaryVariantHoldInput<TSelected>(string VariantCode, IReadOnlyList<AncillaryProductHoldInput<TSelected>> Products)
        where TSelected : class;
}
