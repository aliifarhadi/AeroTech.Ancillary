using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAssistedTravelRule.Backoffice
{
    public sealed class BackofficeChangeProvisionAssistedTravelRuleCommandHandler
        : IRequestHandler<BackofficeChangeProvisionAssistedTravelRuleCommand, ProvisionResult>
    {
        private readonly IChangeProvisionAssistedTravelRuleService _service;

        public BackofficeChangeProvisionAssistedTravelRuleCommandHandler(IChangeProvisionAssistedTravelRuleService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionAssistedTravelRuleCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
