using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AssistedTravelSelectionOptions(
        string? AssistanceSsrCode,
        string? InfantRef,
        string? GuardianRef) : ISelectionOptions;
}
