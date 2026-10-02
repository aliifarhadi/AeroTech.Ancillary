namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public interface IDefineAncillaryPriceRuleCommand
    {
        int OwnerAirlineId { get; }

        string ProductRef { get; }

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
