using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate
{
    public sealed class InventoryPolicyRepository : IInventoryPolicyRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public InventoryPolicyRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryInventoryPolicy policy, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryInventoryPolicies.AddAsync(policy, cancellationToken);

        public Task<AncillaryInventoryPolicy?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryInventoryPolicies
                .Include(policy => policy.PassengerUsageLimits)
                .FirstOrDefaultAsync(policy => policy.Id == id, cancellationToken);

        public Task<bool> HasCurrentAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryInventoryPolicies.AnyAsync(
                policy => policy.OwnerAirlineId == ownerAirlineId
                          && policy.ServiceDefinitionRef == serviceDefinitionRef
                          && policy.Status != InventoryRecordStatus.Retired,
                cancellationToken);

        public async Task<IReadOnlyList<UsageConsumptionUnit>> FindFamilyConsumptionUnitsAsync(
            int ownerAirlineId,
            string countingFamilyCode,
            long exceptPolicyId,
            CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryInventoryPolicies
                .Where(policy => policy.OwnerAirlineId == ownerAirlineId
                                 && policy.Id != exceptPolicyId
                                 && (policy.Status == InventoryRecordStatus.Active || policy.Status == InventoryRecordStatus.Suspended))
                .SelectMany(policy => policy.PassengerUsageLimits)
                .Where(limit => limit.CountingFamilyCode == countingFamilyCode)
                .Select(limit => limit.ConsumptionUnit)
                .Distinct()
                .ToListAsync(cancellationToken);
    }
}
