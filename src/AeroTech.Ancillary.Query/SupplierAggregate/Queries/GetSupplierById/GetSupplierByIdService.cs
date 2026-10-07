using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById
{
    public sealed class GetSupplierByIdService : IGetSupplierByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetSupplierByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeSupplierDto> ExecuteAsync(long supplierId, CancellationToken cancellationToken = default)
        {
            var supplier = await _dbContext.Suppliers
                               .AsNoTracking()
                               .FirstOrDefaultAsync(row => row.Id == supplierId, cancellationToken)
                           ?? throw ExceptionFactory.SupplierNotFound();

            return SupplierMapper.ToBackofficeSupplier(supplier);
        }
    }
}
