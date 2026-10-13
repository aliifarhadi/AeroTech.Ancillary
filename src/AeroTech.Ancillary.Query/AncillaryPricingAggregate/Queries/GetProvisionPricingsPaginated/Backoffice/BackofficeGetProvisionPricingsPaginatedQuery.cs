using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated.Backoffice
{
    public sealed class BackofficeGetProvisionPricingsPaginatedQuery
        : PaginationQuery, IRequest<GridData<ProvisionPricingRowDto>>, IProvisionPricingsPaginatedQuery
    {
        public long AncillaryProvisionId { get; set; }
    }
}
