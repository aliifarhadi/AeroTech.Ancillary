using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule
{
    public sealed record PriceRuleLine(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal Amount);

    public sealed record PriceRuleConditionsInput(
        IReadOnlyList<PassengerTypeCode>? PassengerTypes,
        IReadOnlyList<int>? OriginAirportIds,
        IReadOnlyList<int>? DestinationAirportIds);
}
