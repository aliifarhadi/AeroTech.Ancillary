namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public interface ITypedSelectionVerifier
    {
        Task<VerifiedTypedSelection> VerifyAsync(TypedSelectionVerification verification, CancellationToken cancellationToken = default);
    }
}
