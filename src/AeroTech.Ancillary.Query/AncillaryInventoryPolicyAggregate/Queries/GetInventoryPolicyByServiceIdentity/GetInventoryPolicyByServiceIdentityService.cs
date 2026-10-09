using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity
{
    public sealed class GetInventoryPolicyByServiceIdentityService : IGetInventoryPolicyByServiceIdentityService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetInventoryPolicyByServiceIdentityService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<BackofficeInventoryPolicyDto> ExecuteAsync(string serviceDefinitionRef, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var policy = await _dbContext.AncillaryInventoryPolicies
                             .AsNoTracking()
                             .FirstOrDefaultAsync(
                                 row => row.OwnerAirlineId == ownerAirlineId
                                        && row.ServiceDefinitionRef == serviceDefinitionRef
                                        && row.Status != InventoryRecordStatus.Retired,
                                 cancellationToken)
                         ?? throw ExceptionFactory.InventoryPolicyNotFound();
            var limits = await _dbContext.AncillaryInventoryPassengerUsageLimits
                .AsNoTracking()
                .Where(row => row.InventoryPolicyId == policy.Id)
                .ToListAsync(cancellationToken);

            return InventoryPolicyMapper.ToBackofficePolicy(policy, limits);
        }
    }
}
