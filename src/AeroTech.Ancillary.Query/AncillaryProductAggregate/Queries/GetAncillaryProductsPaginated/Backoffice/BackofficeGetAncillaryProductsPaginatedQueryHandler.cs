using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated.Backoffice
{
    public sealed class BackofficeGetAncillaryProductsPaginatedQueryHandler
        : IRequestHandler<BackofficeGetAncillaryProductsPaginatedQuery, GridData<AncillaryProductPaginatedRowDto>>
    {
        private readonly IGetAncillaryProductsPaginatedService _service;

        public BackofficeGetAncillaryProductsPaginatedQueryHandler(IGetAncillaryProductsPaginatedService service) => _service = service;

        public Task<GridData<AncillaryProductPaginatedRowDto>> Handle(
            BackofficeGetAncillaryProductsPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
