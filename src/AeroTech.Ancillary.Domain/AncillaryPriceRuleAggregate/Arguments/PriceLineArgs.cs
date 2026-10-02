using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments
{
    public sealed record PriceLineArgs(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal Amount);
}
