using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record SelectionVerification(
        AuthorizationSurface Surface,
        string? OfferId,
        IReadOnlyList<SubmittedSelection> Selections);
}
