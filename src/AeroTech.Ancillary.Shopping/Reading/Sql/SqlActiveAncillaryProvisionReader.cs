using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Persistence;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    public sealed class SqlActiveAncillaryProvisionReader : IActiveAncillaryProvisionReader
    {
        private readonly AncillaryDbContext _dbContext;

        public SqlActiveAncillaryProvisionReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<AncillaryProvision>> ListActiveAsync(
            IReadOnlyCollection<long> serviceDefinitionIds,
            long pointOfSaleId,
            CancellationToken cancellationToken = default)
        {
            if (serviceDefinitionIds.Count == 0)
                return [];

            return await _dbContext.AncillaryProvisions
                .AsNoTracking()
                .AsSplitQuery()
                .WithChildren(_dbContext)
                .Where(provision => serviceDefinitionIds.Contains(provision.ServiceDefinitionId)
                                    && provision.Status == ProvisionStatus.Active
                                    && provision.SalesRestrictions!.PointsOfSale.Count == 1
                                    && provision.SalesRestrictions.PointsOfSale.Any(pointOfSale => pointOfSale.PointOfSaleId == pointOfSaleId))
                .OrderBy(provision => provision.ServiceDefinitionId)
                .ThenBy(provision => provision.Sequence)
                .ToListAsync(cancellationToken);
        }
    }
}
