using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated.Backoffice
{
    public sealed class BackofficeGetInventoryPoliciesPaginatedQueryHandler : IRequestHandler<BackofficeGetInventoryPoliciesPaginatedQuery, GridData<InventoryPolicyPaginatedRowDto>>
    {
        private readonly IGetInventoryPoliciesPaginatedService _service;

        public BackofficeGetInventoryPoliciesPaginatedQueryHandler(IGetInventoryPoliciesPaginatedService service) => _service = service;

        public Task<GridData<InventoryPolicyPaginatedRowDto>> Handle(BackofficeGetInventoryPoliciesPaginatedQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
