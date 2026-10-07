using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using MediatR;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier.Backoffice
{
    public sealed record BackofficeRetireSupplierCommand(long SupplierId)
        : IRequest<SupplierResult>, IRetireSupplierCommand;
}
