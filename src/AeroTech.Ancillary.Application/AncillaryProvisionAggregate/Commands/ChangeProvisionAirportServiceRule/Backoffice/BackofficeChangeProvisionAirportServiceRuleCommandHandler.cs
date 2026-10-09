using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAirportServiceRule.Backoffice
{
    public sealed class BackofficeChangeProvisionAirportServiceRuleCommandHandler
        : IRequestHandler<BackofficeChangeProvisionAirportServiceRuleCommand, ProvisionResult>
    {
        private readonly IChangeProvisionAirportServiceRuleService _service;

        public BackofficeChangeProvisionAirportServiceRuleCommandHandler(IChangeProvisionAirportServiceRuleService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionAirportServiceRuleCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
