using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById
{
    public sealed class GetAncillaryProductByIdService : IGetAncillaryProductByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryProductByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeAncillaryProductDto> ExecuteAsync(long ancillaryProductId, CancellationToken cancellationToken = default)
        {
            var product = await _dbContext.AncillaryProducts
                              .AsNoTracking()
                              .FirstOrDefaultAsync(row => row.Id == ancillaryProductId, cancellationToken)
                          ?? throw ExceptionFactory.AncillaryProductNotFound();

            return AncillaryProductMapper.ToBackofficeProduct(product);
        }
    }
}
