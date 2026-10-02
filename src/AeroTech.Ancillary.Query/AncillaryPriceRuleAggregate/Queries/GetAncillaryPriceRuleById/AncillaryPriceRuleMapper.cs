using System.Globalization;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById
{
    public static class AncillaryPriceRuleMapper
    {
        public static BackofficeAncillaryPriceRuleDto ToBackofficePriceRule(AncillaryPriceRuleReadModel rule, IEnumerable<PriceLineReadModel> lines)
            => new(
                rule.Id,
                rule.OwnerAirlineId,
                rule.ProductRef,
                rule.Priority,
                rule.CurrencyId,
                ToLines(lines),
                rule.SalesFrom,
                rule.SalesTo,
                rule.TravelFrom,
                rule.TravelTo,
                ToConditions(rule),
                EnumValueDto.Of(rule.Status),
                rule.CreatedAt);

        public static AncillaryPriceRulePaginatedRowDto ToPaginatedRow(AncillaryPriceRuleReadModel rule, IEnumerable<PriceLineReadModel> lines)
            => new()
            {
                Id = rule.Id.ToString(CultureInfo.InvariantCulture),
                OwnerAirlineId = rule.OwnerAirlineId,
                ProductRef = rule.ProductRef,
                Priority = rule.Priority,
                CurrencyId = rule.CurrencyId,
                Lines = ToLines(lines),
                SalesFrom = rule.SalesFrom,
                SalesTo = rule.SalesTo,
                TravelFrom = rule.TravelFrom,
                TravelTo = rule.TravelTo,
                Conditions = ToConditions(rule),
                Status = EnumValueDto.Of(rule.Status),
                CreatedAt = rule.CreatedAt
            };

        private static IReadOnlyList<PriceLineDto> ToLines(IEnumerable<PriceLineReadModel> lines)
            => lines
                .OrderBy(line => line.Id)
                .Select(line => new PriceLineDto(EnumValueDto.Of(line.Category), line.Code, line.Name, line.Amount))
                .ToList();

        private static PriceRuleConditionsDto ToConditions(AncillaryPriceRuleReadModel rule)
            => new(
                rule.PassengerTypes?.Select(passengerType => EnumValueDto.Of(passengerType)).ToList(),
                rule.OriginAirportIds,
                rule.DestinationAirportIds);
    }
}
