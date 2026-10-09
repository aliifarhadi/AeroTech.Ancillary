using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy.Backoffice
{
    public sealed class BackofficeActivateInventoryPolicyCommandHandler
        : IRequestHandler<BackofficeActivateInventoryPolicyCommand, InventoryPolicyResult>
    {
        private readonly IActivateInventoryPolicyService _service;

        public BackofficeActivateInventoryPolicyCommandHandler(IActivateInventoryPolicyService service) => _service = service;

        public Task<InventoryPolicyResult> Handle(BackofficeActivateInventoryPolicyCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
