using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated.Backoffice
{
    public sealed class BackofficeGetInventoryPoliciesPaginatedQuery
        : PaginationQuery, IRequest<GridData<InventoryPolicyPaginatedRowDto>>, IGetInventoryPoliciesPaginatedQuery
    {
        public string? ServiceDefinitionRef { get; set; }

        public InventoryAuthority? Authority { get; set; }

        public LocalInventoryPattern? LocalPattern { get; set; }

        public InventoryRecordStatus? Status { get; set; }
    }
}
