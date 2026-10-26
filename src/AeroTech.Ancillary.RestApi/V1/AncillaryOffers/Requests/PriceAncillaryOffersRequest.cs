using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PriceAncillaryOffersRequest(string? OfferId, IReadOnlyList<AncillarySelectionLine> Selections);
}
