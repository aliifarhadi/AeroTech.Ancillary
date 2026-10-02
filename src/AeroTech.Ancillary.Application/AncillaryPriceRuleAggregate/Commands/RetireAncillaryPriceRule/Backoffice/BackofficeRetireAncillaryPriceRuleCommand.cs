using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule.Backoffice
{
    public sealed record BackofficeRetireAncillaryPriceRuleCommand(
        long AncillaryPriceRuleId) : IRequest<AncillaryPriceRuleResult>, IRetireAncillaryPriceRuleCommand;
}
