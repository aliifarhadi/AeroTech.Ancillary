using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Projection
{
    internal static class AncillaryPriceRuleReadModelSnapshotFactory
    {
        public static AncillaryPriceRuleReadModelSnapshot ToReadModelSnapshot(this AncillaryPriceRule rule)
            => new(
                rule.Id,
                rule.OwnerAirlineId,
                rule.ProductRef,
                rule.Priority,
                rule.CurrencyId,
                rule.SalesFrom,
                rule.SalesTo,
                rule.TravelFrom,
                rule.TravelTo,
                rule.Conditions.PassengerTypes,
                rule.Conditions.OriginAirportIds,
                rule.Conditions.DestinationAirportIds,
                rule.Status,
                rule.CreatedAt)
            {
                Lines = rule.Lines
                    .Select(line => new PriceLineSnapshot(line.Id, line.Category, line.Code, line.Name, line.Amount))
                    .ToList()
            };
    }
}
