using AeroTech.Ancillary.Query.SupplierAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById.Backoffice
{
    public sealed record BackofficeGetSupplierByIdQuery(long SupplierId) : IRequest<BackofficeSupplierDto>;
}
