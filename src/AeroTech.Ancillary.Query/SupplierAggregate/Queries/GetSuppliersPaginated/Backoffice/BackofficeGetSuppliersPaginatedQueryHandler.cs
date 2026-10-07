using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated.Backoffice
{
    public sealed class BackofficeGetSuppliersPaginatedQueryHandler
        : IRequestHandler<BackofficeGetSuppliersPaginatedQuery, GridData<SupplierPaginatedRowDto>>
    {
        private readonly IGetSuppliersPaginatedService _service;

        public BackofficeGetSuppliersPaginatedQueryHandler(IGetSuppliersPaginatedService service) => _service = service;

        public Task<GridData<SupplierPaginatedRowDto>> Handle(
            BackofficeGetSuppliersPaginatedQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
