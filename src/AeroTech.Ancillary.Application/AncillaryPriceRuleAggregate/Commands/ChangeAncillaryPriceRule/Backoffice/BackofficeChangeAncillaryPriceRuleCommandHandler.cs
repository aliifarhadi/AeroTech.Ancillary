using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule.Backoffice
{
    public sealed class BackofficeChangeAncillaryPriceRuleCommandHandler : IRequestHandler<BackofficeChangeAncillaryPriceRuleCommand, AncillaryPriceRuleResult>
    {
        private readonly IChangeAncillaryPriceRuleService _service;

        public BackofficeChangeAncillaryPriceRuleCommandHandler(IChangeAncillaryPriceRuleService service) => _service = service;

        public Task<AncillaryPriceRuleResult> Handle(BackofficeChangeAncillaryPriceRuleCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
