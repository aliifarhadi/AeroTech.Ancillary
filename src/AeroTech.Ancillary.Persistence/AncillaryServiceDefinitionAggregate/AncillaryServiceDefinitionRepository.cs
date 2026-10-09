using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryServiceDefinitionAggregate
{
    public sealed class AncillaryServiceDefinitionRepository : IAncillaryServiceDefinitionRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryServiceDefinitionRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryServiceDefinition definition, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryServiceDefinitions.AddAsync(definition, cancellationToken);

        public Task<AncillaryServiceDefinition?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryServiceDefinitions.AsSplitQuery().FirstOrDefaultAsync(definition => definition.Id == id, cancellationToken);

        public async Task<int> MaxVersionAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryServiceDefinitions
                .Where(definition => definition.OwnerAirlineId == ownerAirlineId
                                     && definition.ServiceDefinitionRef == serviceDefinitionRef)
                .Select(definition => (int?)definition.Version)
                .MaxAsync(cancellationToken) ?? 0;

        public async Task<IReadOnlyList<AncillaryServiceDefinition>> ListVersionsAsync(
            int ownerAirlineId,
            string serviceDefinitionRef,
            CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryServiceDefinitions
                .Where(definition => definition.OwnerAirlineId == ownerAirlineId
                                     && definition.ServiceDefinitionRef == serviceDefinitionRef)
                .OrderBy(definition => definition.Version)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

        public Task<bool> HasActiveAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryServiceDefinitions.AnyAsync(
                definition => definition.OwnerAirlineId == ownerAirlineId
                              && definition.ServiceDefinitionRef == serviceDefinitionRef
                              && definition.Status == ServiceDefinitionStatus.Active,
                cancellationToken);
    }
}
