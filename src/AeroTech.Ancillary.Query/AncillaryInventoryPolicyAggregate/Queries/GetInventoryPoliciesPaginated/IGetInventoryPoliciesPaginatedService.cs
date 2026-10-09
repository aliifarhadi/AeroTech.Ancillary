using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated
{
    public interface IGetInventoryPoliciesPaginatedService
    {
        Task<GridData<InventoryPolicyPaginatedRowDto>> ExecuteAsync(IGetInventoryPoliciesPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
