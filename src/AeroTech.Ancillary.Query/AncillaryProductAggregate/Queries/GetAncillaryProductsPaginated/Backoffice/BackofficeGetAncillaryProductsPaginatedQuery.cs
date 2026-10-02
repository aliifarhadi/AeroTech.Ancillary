using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryProductsPaginatedQuery
        : PaginationQuery, IRequest<GridData<AncillaryProductPaginatedRowDto>>, IAncillaryProductsPaginatedQuery
    {
        public int? OwnerAirlineId { get; set; }
        public string? ProductRef { get; set; }
        public AncillaryProductType? Type { get; set; }
        public AncillaryProductStatus? Status { get; set; }
    }
}
