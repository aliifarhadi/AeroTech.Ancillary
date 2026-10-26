using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record SelectedDimensionsInput(decimal LengthCm, decimal WidthCm, decimal HeightCm);
}
