namespace AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts
{
    public interface IServiceReservationRepository
    {
        Task AddAsync(ServiceReservation reservation, CancellationToken cancellationToken = default);

        Task<ServiceReservation?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<ServiceReservation?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

        void Update(ServiceReservation reservation);
    }
}
