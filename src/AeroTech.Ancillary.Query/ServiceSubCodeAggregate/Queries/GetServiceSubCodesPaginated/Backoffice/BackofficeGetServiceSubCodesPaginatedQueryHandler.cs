using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated.Backoffice
{
    public sealed class BackofficeGetServiceSubCodesPaginatedQueryHandler
        : IRequestHandler<BackofficeGetServiceSubCodesPaginatedQuery, GridData<ServiceSubCodePaginatedRowDto>>
    {
        private readonly IGetServiceSubCodesPaginatedService _service;

        public BackofficeGetServiceSubCodesPaginatedQueryHandler(IGetServiceSubCodesPaginatedService service) => _service = service;

        public Task<GridData<ServiceSubCodePaginatedRowDto>> Handle(
            BackofficeGetServiceSubCodesPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
