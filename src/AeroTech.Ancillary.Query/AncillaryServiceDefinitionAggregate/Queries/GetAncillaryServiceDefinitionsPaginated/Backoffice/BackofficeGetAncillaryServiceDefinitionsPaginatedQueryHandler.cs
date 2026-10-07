using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryServiceDefinitionsPaginatedQueryHandler
        : IRequestHandler<BackofficeGetAncillaryServiceDefinitionsPaginatedQuery, GridData<ServiceDefinitionPaginatedRowDto>>
    {
        private readonly IGetAncillaryServiceDefinitionsPaginatedService _service;

        public BackofficeGetAncillaryServiceDefinitionsPaginatedQueryHandler(IGetAncillaryServiceDefinitionsPaginatedService service)
            => _service = service;

        public Task<GridData<ServiceDefinitionPaginatedRowDto>> Handle(
            BackofficeGetAncillaryServiceDefinitionsPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
