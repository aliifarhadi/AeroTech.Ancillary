using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryInventoryPolicyAggregate
{
    public sealed class InventoryPolicyQueryDbSynchronizer : IInventoryPolicyQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public InventoryPolicyQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(InventoryPolicyReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var policy = await _dbContext.AncillaryInventoryPolicies.FirstOrDefaultAsync(row => row.Id == snapshot.PolicyId, cancellationToken);

            if (policy is null)
            {
                policy = new InventoryPolicyReadModel { Id = snapshot.PolicyId };
                _dbContext.AncillaryInventoryPolicies.Add(policy);
            }

            policy.OwnerAirlineId = snapshot.OwnerAirlineId;
            policy.ServiceDefinitionRef = snapshot.ServiceDefinitionRef;
            policy.ServiceDefinitionId = snapshot.ServiceDefinitionId;
            policy.Authority = snapshot.Authority;
            policy.LocalPattern = snapshot.LocalPattern;
            policy.ProviderKey = snapshot.ProviderKey;
            policy.CountResourceId = snapshot.CountResourceId;
            policy.CountPerAcceptedUnit = snapshot.CountPerAcceptedUnit;
            policy.CountUnit = snapshot.CountUnit;
            policy.WeightResourceId = snapshot.WeightResourceId;
            policy.WeightConsumptionMode = snapshot.WeightConsumptionMode;
            policy.WeightFixedKgPerUnit = snapshot.WeightFixedKgPerUnit;
            policy.SlotFacilityId = snapshot.SlotFacilityId;
            policy.SlotOccupancyMinutes = snapshot.SlotOccupancyMinutes;
            policy.SlotPeoplePerAcceptedUnit = snapshot.SlotPeoplePerAcceptedUnit;
            policy.Status = snapshot.Status;
            policy.Version = snapshot.Version;
            policy.CreatedAt = snapshot.CreatedAt;
            policy.UpdatedAt = snapshot.UpdatedAt;
            policy.ActivatedAt = snapshot.ActivatedAt;
            policy.SuspendedAt = snapshot.SuspendedAt;
            policy.RetiredAt = snapshot.RetiredAt;
            policy.LastUpdateTime = _clock.GetDateTime();

            var storedLimits = await _dbContext.AncillaryInventoryPassengerUsageLimits
                .Where(row => row.InventoryPolicyId == snapshot.PolicyId)
                .ToListAsync(cancellationToken);

            _dbContext.AncillaryInventoryPassengerUsageLimits.RemoveRange(
                storedLimits.Where(row => snapshot.PassengerUsageLimits.All(limit => limit.LimitId != row.Id)));

            foreach (var limit in snapshot.PassengerUsageLimits)
            {
                var row = storedLimits.FirstOrDefault(stored => stored.Id == limit.LimitId);

                if (row is null)
                {
                    row = new InventoryPassengerUsageLimitReadModel { Id = limit.LimitId, InventoryPolicyId = snapshot.PolicyId };
                    _dbContext.AncillaryInventoryPassengerUsageLimits.Add(row);
                }

                row.LimitScope = limit.LimitScope;
                row.MaxUnits = limit.MaxUnits;
                row.CountingFamilyCode = limit.CountingFamilyCode;
            }
        }
    }
}
