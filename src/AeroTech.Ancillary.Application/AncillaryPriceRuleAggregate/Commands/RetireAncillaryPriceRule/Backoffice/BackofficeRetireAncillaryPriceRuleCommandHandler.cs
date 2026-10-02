using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule.Backoffice
{
    public sealed class BackofficeRetireAncillaryPriceRuleCommandHandler : IRequestHandler<BackofficeRetireAncillaryPriceRuleCommand, AncillaryPriceRuleResult>
    {
        private readonly IRetireAncillaryPriceRuleService _service;

        public BackofficeRetireAncillaryPriceRuleCommandHandler(IRetireAncillaryPriceRuleService service) => _service = service;

        public Task<AncillaryPriceRuleResult> Handle(BackofficeRetireAncillaryPriceRuleCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
