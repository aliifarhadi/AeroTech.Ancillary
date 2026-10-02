using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryProductAggregate
{
    public sealed class AncillaryProductRepository : IAncillaryProductRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryProductRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryProduct product, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryProducts.AddAsync(product, cancellationToken);

        public Task<AncillaryProduct?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProducts.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

        public Task<bool> ExistsAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProducts
                .AnyAsync(product => product.OwnerAirlineId == ownerAirlineId && product.ProductRef == productRef, cancellationToken);

        public Task<bool> DraftExistsAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProducts
                .AnyAsync(product => product.OwnerAirlineId == ownerAirlineId
                                     && product.ProductRef == productRef
                                     && product.Status == AncillaryProductStatus.Draft,
                    cancellationToken);

        public Task<int> HighestVersionAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProducts
                .Where(product => product.OwnerAirlineId == ownerAirlineId && product.ProductRef == productRef)
                .MaxAsync(product => product.Version, cancellationToken);

        public Task<AncillaryProduct?> FindOfferedVersionAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProducts
                .FirstOrDefaultAsync(product => product.OwnerAirlineId == ownerAirlineId
                                                && product.ProductRef == productRef
                                                && (product.Status == AncillaryProductStatus.Active
                                                    || product.Status == AncillaryProductStatus.Suspended),
                    cancellationToken);

        public async Task<IReadOnlyList<AncillaryProduct>> ListActiveAsync(CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryProducts
                .AsNoTracking()
                .Where(product => product.Status == AncillaryProductStatus.Active)
                .ToListAsync(cancellationToken);
    }
}
