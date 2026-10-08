using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase.Backoffice
{
    public sealed class BackofficeChangeProvisionAdvancePurchaseCommandHandler
        : IRequestHandler<BackofficeChangeProvisionAdvancePurchaseCommand, ProvisionResult>
    {
        private readonly IChangeProvisionAdvancePurchaseService _service;

        public BackofficeChangeProvisionAdvancePurchaseCommandHandler(IChangeProvisionAdvancePurchaseService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionAdvancePurchaseCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
