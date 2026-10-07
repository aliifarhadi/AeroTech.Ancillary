using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public sealed class GetAncillaryProvisionByIdService : IGetAncillaryProvisionByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryProvisionByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeProvisionDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default)
        {
            var provision = await _dbContext.AncillaryProvisions
                                .AsNoTracking()
                                .FirstOrDefaultAsync(row => row.Id == provisionId, cancellationToken)
                            ?? throw ExceptionFactory.ProvisionNotFound();

            var priceLines = await _dbContext.AncillaryProvisionPriceLines
                .AsNoTracking()
                .Where(line => line.AncillaryProvisionId == provisionId)
                .ToListAsync(cancellationToken);

            return AncillaryProvisionMapper.ToBackofficeProvision(provision, priceLines);
        }
    }
}
