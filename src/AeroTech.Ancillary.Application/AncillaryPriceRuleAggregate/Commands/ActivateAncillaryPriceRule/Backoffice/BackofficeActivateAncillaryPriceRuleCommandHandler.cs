using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule.Backoffice
{
    public sealed class BackofficeActivateAncillaryPriceRuleCommandHandler : IRequestHandler<BackofficeActivateAncillaryPriceRuleCommand, AncillaryPriceRuleResult>
    {
        private readonly IActivateAncillaryPriceRuleService _service;

        public BackofficeActivateAncillaryPriceRuleCommandHandler(IActivateAncillaryPriceRuleService service) => _service = service;

        public Task<AncillaryPriceRuleResult> Handle(BackofficeActivateAncillaryPriceRuleCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
