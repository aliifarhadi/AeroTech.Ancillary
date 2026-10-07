using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using MediatR;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier.Backoffice
{
    public sealed class BackofficeRetireSupplierCommandHandler
        : IRequestHandler<BackofficeRetireSupplierCommand, SupplierResult>
    {
        private readonly IRetireSupplierService _service;

        public BackofficeRetireSupplierCommandHandler(IRetireSupplierService service) => _service = service;

        public Task<SupplierResult> Handle(BackofficeRetireSupplierCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
