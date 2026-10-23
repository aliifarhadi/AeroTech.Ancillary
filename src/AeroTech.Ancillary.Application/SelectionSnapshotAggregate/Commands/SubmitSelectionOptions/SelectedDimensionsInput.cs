using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record SelectedDimensionsInput(decimal LengthCm, decimal WidthCm, decimal HeightCm);
}
