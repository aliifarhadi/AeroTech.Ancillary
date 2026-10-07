using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryProvisionsPaginatedQueryHandler
        : IRequestHandler<BackofficeGetAncillaryProvisionsPaginatedQuery, GridData<ProvisionPaginatedRowDto>>
    {
        private readonly IGetAncillaryProvisionsPaginatedService _service;

        public BackofficeGetAncillaryProvisionsPaginatedQueryHandler(IGetAncillaryProvisionsPaginatedService service)
            => _service = service;

        public Task<GridData<ProvisionPaginatedRowDto>> Handle(
            BackofficeGetAncillaryProvisionsPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
