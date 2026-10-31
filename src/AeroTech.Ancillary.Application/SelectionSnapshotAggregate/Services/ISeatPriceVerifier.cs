namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public interface ISeatPriceVerifier
    {
        Task<IReadOnlyList<VerifiedSelection>> VerifyAsync(SeatPriceVerification verification, CancellationToken cancellationToken = default);

        Task<VerifiedOrderSelections> VerifyAsync(OrderSeatPriceVerification verification, CancellationToken cancellationToken = default);
    }
}
