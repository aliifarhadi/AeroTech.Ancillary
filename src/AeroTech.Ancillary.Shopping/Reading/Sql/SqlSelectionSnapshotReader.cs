using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    public sealed class SqlSelectionSnapshotReader : ISelectionSnapshotReader
    {
        private readonly AncillaryDbContext _dbContext;

        public SqlSelectionSnapshotReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<SelectionSnapshot>> FindAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken = default)
        {
            if (ids.Count == 0)
                return [];

            return await _dbContext.SelectionSnapshots
                .AsNoTracking()
                .Where(snapshot => ids.Contains(snapshot.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
