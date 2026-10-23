using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PrioritySelectionOptions(bool? OptIn) : ISelectionOptions;
}
