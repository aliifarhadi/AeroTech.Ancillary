using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy.Backoffice
{
    public sealed class BackofficeChangeInventoryPolicyCommandHandler
        : IRequestHandler<BackofficeChangeInventoryPolicyCommand, InventoryPolicyResult>
    {
        private readonly IChangeInventoryPolicyService _service;

        public BackofficeChangeInventoryPolicyCommandHandler(IChangeInventoryPolicyService service) => _service = service;

        public Task<InventoryPolicyResult> Handle(BackofficeChangeInventoryPolicyCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
