using MediatR;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier.Backoffice
{
    public sealed class BackofficeRegisterSupplierCommandHandler : IRequestHandler<BackofficeRegisterSupplierCommand, SupplierResult>
    {
        private readonly IRegisterSupplierService _service;

        public BackofficeRegisterSupplierCommandHandler(IRegisterSupplierService service) => _service = service;

        public Task<SupplierResult> Handle(BackofficeRegisterSupplierCommand command, CancellationToken cancellationToken)
            => _service.RegisterAsync(command, cancellationToken);
    }
}
