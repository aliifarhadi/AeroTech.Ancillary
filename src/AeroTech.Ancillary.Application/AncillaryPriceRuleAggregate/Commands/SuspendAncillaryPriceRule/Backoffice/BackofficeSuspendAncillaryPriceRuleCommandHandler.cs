using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule.Backoffice
{
    public sealed class BackofficeSuspendAncillaryPriceRuleCommandHandler : IRequestHandler<BackofficeSuspendAncillaryPriceRuleCommand, AncillaryPriceRuleResult>
    {
        private readonly ISuspendAncillaryPriceRuleService _service;

        public BackofficeSuspendAncillaryPriceRuleCommandHandler(ISuspendAncillaryPriceRuleService service) => _service = service;

        public Task<AncillaryPriceRuleResult> Handle(BackofficeSuspendAncillaryPriceRuleCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
