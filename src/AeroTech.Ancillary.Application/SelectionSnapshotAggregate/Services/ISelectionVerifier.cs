namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public interface ISelectionVerifier
    {
        Task<IReadOnlyList<VerifiedSelection>> VerifyAsync(SelectionVerification verification, CancellationToken cancellationToken = default);
    }
}
