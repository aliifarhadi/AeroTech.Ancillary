using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Contracts;

namespace AeroTech.Ancillary.Persistence.SelectionSnapshotAggregate
{
    public sealed class SelectionSnapshotRepository : ISelectionSnapshotRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public SelectionSnapshotRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(SelectionSnapshot snapshot, CancellationToken cancellationToken = default)
            => await _dbContext.SelectionSnapshots.AddAsync(snapshot, cancellationToken);
    }
}
