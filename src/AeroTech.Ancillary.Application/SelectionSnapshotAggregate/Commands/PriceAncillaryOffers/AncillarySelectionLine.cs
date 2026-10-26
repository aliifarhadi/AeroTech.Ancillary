using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AncillarySelectionLine(string? OfferItemId = null, int? Quantity = null, SelectionOptionsInput? Options = null);
}
