using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryPricingAggregate
{
    public sealed class AncillaryPricingRepository : IAncillaryPricingRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryPricingRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryPricing pricing, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryPricings.AddAsync(pricing, cancellationToken);

        public Task<AncillaryPricing?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryPricings
                .Include(pricing => pricing.Rates)
                .ThenInclude(rate => rate.Components)
                .FirstOrDefaultAsync(pricing => pricing.Id == id, cancellationToken);

        public Task<AncillaryPricing?> FindActiveAsync(long ancillaryProvisionId, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryPricings
                .Include(pricing => pricing.Rates)
                .ThenInclude(rate => rate.Components)
                .FirstOrDefaultAsync(
                    pricing => pricing.AncillaryProvisionId == ancillaryProvisionId && pricing.Status == PricingStatus.Active,
                    cancellationToken);

        public async Task<int> MaxVersionAsync(long ancillaryProvisionId, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryPricings
                .Where(pricing => pricing.AncillaryProvisionId == ancillaryProvisionId)
                .Select(pricing => (int?)pricing.Version)
                .MaxAsync(cancellationToken) ?? 0;

        public async Task<IReadOnlyList<AncillaryPricing>> ListUnassignedAsync(
            IReadOnlyCollection<long> serviceDefinitionIds,
            CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryPricings
                .Include(pricing => pricing.Rates)
                .ThenInclude(rate => rate.Components)
                .Where(pricing => pricing.PricingUnit == null
                                  && _dbContext.AncillaryProvisions.Any(
                                      provision => provision.Id == pricing.AncillaryProvisionId
                                                   && serviceDefinitionIds.Contains(provision.ServiceDefinitionId)))
                .ToListAsync(cancellationToken);
    }
}
