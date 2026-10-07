using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryReservationAggregate
{
    public sealed class AncillaryReservationRepository : IAncillaryReservationRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryReservationRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryReservation reservation, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryReservations.AddAsync(reservation, cancellationToken);

        public Task<AncillaryReservation?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryReservations
                .Include(reservation => reservation.Units)
                .FirstOrDefaultAsync(reservation => reservation.Id == id, cancellationToken);

        public Task<AncillaryReservation?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryReservations
                .AsNoTracking()
                .Include(reservation => reservation.Units)
                .FirstOrDefaultAsync(reservation => reservation.IdempotencyKey == idempotencyKey, cancellationToken);

        public async Task<IReadOnlyList<long>> ReservedOrderServiceIdsAsync(
            IReadOnlyCollection<long> orderServiceIds,
            DateTimeOffset now,
            CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryReservations
                .AsNoTracking()
                .SelectMany(reservation => reservation.Units
                    .Where(unit => orderServiceIds.Contains(unit.OrderServiceId)
                                   && (unit.Status == AncillaryReservationUnitStatus.Confirmed
                                       || (unit.Status == AncillaryReservationUnitStatus.Held
                                           && (reservation.ExpiresAt == null || reservation.ExpiresAt > now)))))
                .Select(unit => unit.OrderServiceId)
                .Distinct()
                .ToListAsync(cancellationToken);

        public void Update(AncillaryReservation reservation) => _dbContext.AncillaryReservations.Update(reservation);
    }
}
