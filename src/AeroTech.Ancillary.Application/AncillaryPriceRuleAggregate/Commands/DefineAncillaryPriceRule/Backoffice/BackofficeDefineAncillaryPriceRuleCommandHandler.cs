using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice
{
    public sealed class BackofficeDefineAncillaryPriceRuleCommandHandler : IRequestHandler<BackofficeDefineAncillaryPriceRuleCommand, AncillaryPriceRuleResult>
    {
        private readonly IDefineAncillaryPriceRuleService _service;

        public BackofficeDefineAncillaryPriceRuleCommandHandler(IDefineAncillaryPriceRuleService service) => _service = service;

        public Task<AncillaryPriceRuleResult> Handle(BackofficeDefineAncillaryPriceRuleCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
