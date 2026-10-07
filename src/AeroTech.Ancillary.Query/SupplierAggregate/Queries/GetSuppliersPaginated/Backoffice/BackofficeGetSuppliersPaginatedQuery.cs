using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated.Backoffice
{
    public sealed class BackofficeGetSuppliersPaginatedQuery
        : PaginationQuery, IRequest<GridData<SupplierPaginatedRowDto>>, ISuppliersPaginatedQuery
    {
        public int? OwnerAirlineId { get; set; }
        public SupplierFulfillmentKind? FulfillmentKind { get; set; }
        public SupplierStatus? Status { get; set; }
        public string? Search { get; set; }
    }
}
