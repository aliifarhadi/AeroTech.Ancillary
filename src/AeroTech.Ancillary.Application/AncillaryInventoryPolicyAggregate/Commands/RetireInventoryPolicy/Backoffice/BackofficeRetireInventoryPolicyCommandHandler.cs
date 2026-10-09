using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy.Backoffice
{
    public sealed class BackofficeRetireInventoryPolicyCommandHandler
        : IRequestHandler<BackofficeRetireInventoryPolicyCommand, InventoryPolicyResult>
    {
        private readonly IRetireInventoryPolicyService _service;

        public BackofficeRetireInventoryPolicyCommandHandler(IRetireInventoryPolicyService service) => _service = service;

        public Task<InventoryPolicyResult> Handle(BackofficeRetireInventoryPolicyCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
