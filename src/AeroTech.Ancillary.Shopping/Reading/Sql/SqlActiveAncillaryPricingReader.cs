using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Persistence;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    public sealed class SqlActiveAncillaryPricingReader : IActiveAncillaryPricingReader
    {
        private readonly AncillaryDbContext _dbContext;

        public SqlActiveAncillaryPricingReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<AncillaryPricing>> ListActiveAsync(IReadOnlyCollection<long> ancillaryProvisionIds, CancellationToken cancellationToken = default)
        {
            if (ancillaryProvisionIds.Count == 0)
                return [];

            return await _dbContext.AncillaryPricings
                .AsNoTracking()
                .AsSplitQuery()
                .WithChildren(_dbContext)
                .Where(pricing => ancillaryProvisionIds.Contains(pricing.AncillaryProvisionId) && pricing.Status == PricingStatus.Active)
                .OrderBy(pricing => pricing.Id)
                .ToListAsync(cancellationToken);
        }
    }
}
