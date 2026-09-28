using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Query._Shared.DbContexts;

namespace AeroTech.Ancillary.Synchronizer._Shared
{
    public sealed class AncillaryUnitOfWork : IUnitOfWork
    {
        private readonly AncillaryDbContext _commandDbContext;
        private readonly AncillaryQueryDbContext _queryDbContext;

        public AncillaryUnitOfWork(AncillaryDbContext commandDbContext, AncillaryQueryDbContext queryDbContext)
        {
            _commandDbContext = commandDbContext;
            _queryDbContext = queryDbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var affected = await _commandDbContext.SaveChangesAsync(cancellationToken);
            await _queryDbContext.SaveChangesAsync(cancellationToken);
            return affected;
        }
    }
}
