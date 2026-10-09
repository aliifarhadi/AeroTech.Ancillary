using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy.Backoffice
{
    public sealed class BackofficeSuspendInventoryPolicyCommandHandler
        : IRequestHandler<BackofficeSuspendInventoryPolicyCommand, InventoryPolicyResult>
    {
        private readonly ISuspendInventoryPolicyService _service;

        public BackofficeSuspendInventoryPolicyCommandHandler(ISuspendInventoryPolicyService service) => _service = service;

        public Task<InventoryPolicyResult> Handle(BackofficeSuspendInventoryPolicyCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
