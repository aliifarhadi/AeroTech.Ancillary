using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryPriceRuleAggregate.Requests
{
    public sealed record ChangeAncillaryPriceRuleRequest(
        int Priority,
        int CurrencyId,
        IReadOnlyList<PriceRuleLine> Lines,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesTo,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        PriceRuleConditionsInput Conditions);
}
