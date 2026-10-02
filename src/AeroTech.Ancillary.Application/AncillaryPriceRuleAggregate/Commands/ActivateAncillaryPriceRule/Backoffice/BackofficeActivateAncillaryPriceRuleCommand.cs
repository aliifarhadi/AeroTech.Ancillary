using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule.Backoffice
{
    public sealed record BackofficeActivateAncillaryPriceRuleCommand(
        long AncillaryPriceRuleId) : IRequest<AncillaryPriceRuleResult>, IActivateAncillaryPriceRuleCommand;
}
