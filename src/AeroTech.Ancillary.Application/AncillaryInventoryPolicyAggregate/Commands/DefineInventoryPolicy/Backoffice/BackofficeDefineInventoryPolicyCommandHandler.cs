using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy.Backoffice
{
    public sealed class BackofficeDefineInventoryPolicyCommandHandler
        : IRequestHandler<BackofficeDefineInventoryPolicyCommand, InventoryPolicyResult>
    {
        private readonly IDefineInventoryPolicyService _service;

        public BackofficeDefineInventoryPolicyCommandHandler(IDefineInventoryPolicyService service) => _service = service;

        public Task<InventoryPolicyResult> Handle(BackofficeDefineInventoryPolicyCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
