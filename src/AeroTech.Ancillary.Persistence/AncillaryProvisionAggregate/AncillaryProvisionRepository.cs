using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class AncillaryProvisionRepository : IAncillaryProvisionRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryProvisionRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryProvision provision, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryProvisions.AddAsync(provision, cancellationToken);

        public Task<AncillaryProvision?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProvisions
                .Include(provision => provision.PriceLines)
                .FirstOrDefaultAsync(provision => provision.Id == id, cancellationToken);

        public Task<AncillaryProvision?> FindActiveAtSequenceAsync(long serviceDefinitionId, int sequence, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryProvisions
                .Include(provision => provision.PriceLines)
                .FirstOrDefaultAsync(
                    provision => provision.ServiceDefinitionId == serviceDefinitionId
                                 && provision.Sequence == sequence
                                 && provision.Status == ProvisionStatus.Active,
                    cancellationToken);
    }
}
