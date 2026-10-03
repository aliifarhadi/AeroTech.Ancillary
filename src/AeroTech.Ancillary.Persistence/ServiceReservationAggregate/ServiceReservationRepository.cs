using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.ServiceReservationAggregate
{
    public sealed class ServiceReservationRepository : IServiceReservationRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public ServiceReservationRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(ServiceReservation reservation, CancellationToken cancellationToken = default)
            => await _dbContext.ServiceReservations.AddAsync(reservation, cancellationToken);

        public Task<ServiceReservation?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.ServiceReservations
                .Include(reservation => reservation.Units)
                .FirstOrDefaultAsync(reservation => reservation.Id == id, cancellationToken);

        public Task<ServiceReservation?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
            => _dbContext.ServiceReservations
                .AsNoTracking()
                .Include(reservation => reservation.Units)
                .FirstOrDefaultAsync(reservation => reservation.IdempotencyKey == idempotencyKey, cancellationToken);

        public void Update(ServiceReservation reservation) => _dbContext.ServiceReservations.Update(reservation);
    }
}
