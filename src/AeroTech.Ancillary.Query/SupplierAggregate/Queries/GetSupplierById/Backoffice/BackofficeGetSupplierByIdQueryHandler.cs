using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById.Backoffice
{
    public sealed class BackofficeGetSupplierByIdQueryHandler : IRequestHandler<BackofficeGetSupplierByIdQuery, BackofficeSupplierDto>
    {
        private readonly IGetSupplierByIdService _service;

        public BackofficeGetSupplierByIdQueryHandler(IGetSupplierByIdService service) => _service = service;

        public Task<BackofficeSupplierDto> Handle(BackofficeGetSupplierByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.SupplierId, cancellationToken);
    }
}
