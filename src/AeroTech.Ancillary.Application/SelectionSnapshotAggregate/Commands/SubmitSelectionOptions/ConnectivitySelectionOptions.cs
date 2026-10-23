using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record ConnectivitySelectionOptions(int? DeviceCount) : ISelectionOptions;
}
