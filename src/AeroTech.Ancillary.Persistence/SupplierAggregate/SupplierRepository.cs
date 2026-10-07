using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.SupplierAggregate
{
    public sealed class SupplierRepository : ISupplierRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public SupplierRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default)
            => await _dbContext.Suppliers.AddAsync(supplier, cancellationToken);

        public Task<Supplier?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.Suppliers.FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);
    }
}
