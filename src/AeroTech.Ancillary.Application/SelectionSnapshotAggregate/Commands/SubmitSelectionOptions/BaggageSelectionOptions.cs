using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record BaggageSelectionOptions(
        string? BagRef,
        decimal? MeasuredWeightKg,
        SelectedDimensionsInput? Dimensions,
        decimal? WeightKg) : ISelectionOptions;
}
