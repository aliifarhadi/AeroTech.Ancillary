using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule.Backoffice
{
    public sealed record BackofficeSuspendAncillaryPriceRuleCommand(
        long AncillaryPriceRuleId) : IRequest<AncillaryPriceRuleResult>, ISuspendAncillaryPriceRuleCommand;
}
