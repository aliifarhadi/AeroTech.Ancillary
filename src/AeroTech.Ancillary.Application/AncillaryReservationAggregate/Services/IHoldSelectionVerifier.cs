namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public interface IHoldSelectionVerifier
    {
        Task<IReadOnlyList<VerifiedHoldSelection>> VerifyAsync(HoldSelectionVerification verification, CancellationToken cancellationToken = default);
    }
}
