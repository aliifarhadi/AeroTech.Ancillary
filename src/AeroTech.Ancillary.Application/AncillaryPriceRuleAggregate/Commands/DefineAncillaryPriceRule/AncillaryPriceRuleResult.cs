using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public sealed record AncillaryPriceRuleResult(
        long Id,
        int OwnerAirlineId,
        string ProductRef,
        int Priority,
        int CurrencyId,
        AncillaryPriceRuleStatus Status);
}
