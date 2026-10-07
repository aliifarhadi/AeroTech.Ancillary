namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts
{
    public interface IAncillaryReservationRepository
    {
        Task AddAsync(AncillaryReservation reservation, CancellationToken cancellationToken = default);

        Task<AncillaryReservation?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<AncillaryReservation?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<long>> ReservedOrderServiceIdsAsync(
            IReadOnlyCollection<long> orderServiceIds,
            DateTimeOffset now,
            CancellationToken cancellationToken = default);

        void Update(AncillaryReservation reservation);
    }
}
