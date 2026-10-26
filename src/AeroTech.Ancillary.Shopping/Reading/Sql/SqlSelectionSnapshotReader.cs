using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    public sealed class SqlSelectionSnapshotReader : ISelectionSnapshotReader
    {
        private readonly AncillaryDbContext _dbContext;

        public SqlSelectionSnapshotReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public Task<SelectionSnapshot?> FindAsync(string id, CancellationToken cancellationToken = default)
            => _dbContext.SelectionSnapshots
                .AsNoTracking()
                .Include(snapshot => snapshot.Lines.OrderBy(line => line.Sequence))
                .SingleOrDefaultAsync(snapshot => snapshot.Id == id, cancellationToken);
    }
}
