using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record OrderSelectionVerification(
        AuthorizationSurface Surface,
        long OrderId,
        IReadOnlyList<SubmittedSelection> Selections);
}
