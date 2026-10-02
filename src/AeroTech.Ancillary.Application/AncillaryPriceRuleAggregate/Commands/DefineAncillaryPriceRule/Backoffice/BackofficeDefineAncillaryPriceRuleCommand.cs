using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice
{
    public sealed record BackofficeDefineAncillaryPriceRuleCommand(
        int OwnerAirlineId,
        string ProductRef,
        int Priority,
        int CurrencyId,
        IReadOnlyList<PriceRuleLine> Lines,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesTo,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        PriceRuleConditionsInput Conditions) : IRequest<AncillaryPriceRuleResult>, IDefineAncillaryPriceRuleCommand;
}
