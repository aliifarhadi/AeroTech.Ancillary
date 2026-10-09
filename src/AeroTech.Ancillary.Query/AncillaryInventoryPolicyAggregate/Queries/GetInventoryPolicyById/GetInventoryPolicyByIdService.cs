using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById
{
    public sealed class GetInventoryPolicyByIdService : IGetInventoryPolicyByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;
        private readonly IInventoryCommercialFactsReader _facts;

        public GetInventoryPolicyByIdService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope, IInventoryCommercialFactsReader facts)
        {
            _dbContext = dbContext;
            _scope = scope;
            _facts = facts;
        }

        public async Task<BackofficeInventoryPolicyDto> ExecuteAsync(long policyId, CancellationToken cancellationToken = default)
        {
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var policy = await _dbContext.AncillaryInventoryPolicies
                             .AsNoTracking()
                             .FirstOrDefaultAsync(row => row.Id == policyId && row.OwnerAirlineId == ownerAirlineId, cancellationToken)
                         ?? throw ExceptionFactory.InventoryPolicyNotFound();
            var limits = await _dbContext.AncillaryInventoryPassengerUsageLimits
                .AsNoTracking()
                .Where(row => row.InventoryPolicyId == policy.Id)
                .ToListAsync(cancellationToken);
            var current = await _facts.FindCurrentAsync(policy.OwnerAirlineId, policy.ServiceDefinitionRef, cancellationToken);

            return InventoryPolicyMapper.ToBackofficePolicy(policy, limits, current?.ServiceDefinitionId);
        }
    }
}
