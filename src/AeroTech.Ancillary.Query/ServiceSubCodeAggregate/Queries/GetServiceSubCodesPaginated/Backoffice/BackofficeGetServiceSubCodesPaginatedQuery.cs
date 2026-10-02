using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated.Backoffice
{
    public sealed class BackofficeGetServiceSubCodesPaginatedQuery
        : PaginationQuery, IRequest<GridData<ServiceSubCodePaginatedRowDto>>, IServiceSubCodesPaginatedQuery
    {
        public int? OwnerAirlineId { get; set; }
        public string? Code { get; set; }
        public ServiceSubCodeSource? Source { get; set; }
        public ServiceSubCodeStatus? Status { get; set; }
    }
}
