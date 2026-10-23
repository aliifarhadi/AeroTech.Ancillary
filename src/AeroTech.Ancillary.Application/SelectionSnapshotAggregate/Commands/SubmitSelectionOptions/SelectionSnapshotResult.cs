namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    public sealed record SelectionSnapshotResult(string SelectionId, DateTimeOffset ExpiresAtUtc);
}
