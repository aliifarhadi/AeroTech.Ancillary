using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule.Backoffice
{
    public sealed record BackofficeChangeAncillaryPriceRuleCommand(
        long AncillaryPriceRuleId,
        int Priority,
        int CurrencyId,
        IReadOnlyList<PriceRuleLine> Lines,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesTo,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        PriceRuleConditionsInput Conditions) : IRequest<AncillaryPriceRuleResult>, IChangeAncillaryPriceRuleCommand;
}
