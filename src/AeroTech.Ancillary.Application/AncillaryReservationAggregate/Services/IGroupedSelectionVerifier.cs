namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public interface IGroupedSelectionVerifier
    {
        Task<IReadOnlyList<VerifiedGroupedSelection>> VerifyAsync(GroupedSelectionVerification verification, CancellationToken cancellationToken = default);
    }
}
