using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated
{
    public sealed class GetInventoryPoliciesPaginatedService : IGetInventoryPoliciesPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetInventoryPoliciesPaginatedService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<GridData<InventoryPolicyPaginatedRowDto>> ExecuteAsync(IGetInventoryPoliciesPaginatedQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var rows = _dbContext.AncillaryInventoryPolicies
                .AsNoTracking()
                .Where(policy => policy.OwnerAirlineId == ownerAirlineId
                                 && (query.ServiceDefinitionRef == null || policy.ServiceDefinitionRef == query.ServiceDefinitionRef)
                                 && (query.Authority == null || policy.Authority == query.Authority)
                                 && (query.LocalPattern == null || policy.LocalPattern == query.LocalPattern)
                                 && (query.Status == null || policy.Status == query.Status));
            var totalCount = await rows.LongCountAsync(cancellationToken);
            var page = await rows
                .OrderBy(policy => policy.ServiceDefinitionRef)
                .ThenByDescending(policy => policy.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GridData<InventoryPolicyPaginatedRowDto>.Create(
                PaginatedList<InventoryPolicyPaginatedRowDto>.Create(page.Select(InventoryPolicyMapper.ToPaginatedRow), pageNumber, pageSize, totalCount));
        }
    }
}
