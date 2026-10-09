using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPetRule.Backoffice
{
    public sealed class BackofficeChangeProvisionPetRuleCommandHandler
        : IRequestHandler<BackofficeChangeProvisionPetRuleCommand, ProvisionResult>
    {
        private readonly IChangeProvisionPetRuleService _service;

        public BackofficeChangeProvisionPetRuleCommandHandler(IChangeProvisionPetRuleService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionPetRuleCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
