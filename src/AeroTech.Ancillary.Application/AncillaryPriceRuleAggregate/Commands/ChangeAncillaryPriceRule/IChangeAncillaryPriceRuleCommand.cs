using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule
{
    public interface IChangeAncillaryPriceRuleCommand
    {
        long AncillaryPriceRuleId { get; }

        int Priority { get; }

        int CurrencyId { get; }

        IReadOnlyList<PriceRuleLine> Lines { get; }

        DateTimeOffset? SalesFrom { get; }

        DateTimeOffset? SalesTo { get; }

        DateOnly? TravelFrom { get; }

        DateOnly? TravelTo { get; }

        PriceRuleConditionsInput Conditions { get; }
    }
}
