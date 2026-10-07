using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryProvisionsPaginatedQuery
        : PaginationQuery, IRequest<GridData<ProvisionPaginatedRowDto>>, IAncillaryProvisionsPaginatedQuery
    {
        public long ServiceDefinitionId { get; set; }
        public long? SupplierId { get; set; }
        public ProvisionStatus? Status { get; set; }
        public ServiceCoverageScope? CoverageScope { get; set; }
        public int? Sequence { get; set; }
        public DateTimeOffset? SalesDate { get; set; }
    }
}
