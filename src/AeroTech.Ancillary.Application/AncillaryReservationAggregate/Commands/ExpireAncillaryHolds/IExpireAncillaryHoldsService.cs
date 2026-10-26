namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds
{
    public interface IExpireAncillaryHoldsService
    {
        Task<IReadOnlyList<long>> ListDueOrderIdsAsync(int batchSize, CancellationToken cancellationToken = default);

        Task<int> ExpireAsync(long orderId, CancellationToken cancellationToken = default);
    }
}
