namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public interface ISeatSelectionVerifier
    {
        Task<IReadOnlyList<VerifiedSeatSelection>> VerifyAsync(SeatSelectionVerification verification, CancellationToken cancellationToken = default);
    }
}
