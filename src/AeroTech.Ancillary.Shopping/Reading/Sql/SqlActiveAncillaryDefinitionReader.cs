using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Persistence;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Shopping.Reading.Sql
{
    public sealed class SqlActiveAncillaryDefinitionReader : IActiveAncillaryDefinitionReader
    {
        private readonly AncillaryDbContext _dbContext;

        public SqlActiveAncillaryDefinitionReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<IReadOnlyList<AncillaryServiceDefinition>> ListActiveAsync(int ownerAirlineId, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryServiceDefinitions
                .AsNoTracking()
                .AsSplitQuery()
                .WithChildren(_dbContext)
                .Where(definition => definition.OwnerAirlineId == ownerAirlineId && definition.Status == ServiceDefinitionStatus.Active)
                .OrderBy(definition => definition.Id)
                .ToListAsync(cancellationToken);
    }
}
