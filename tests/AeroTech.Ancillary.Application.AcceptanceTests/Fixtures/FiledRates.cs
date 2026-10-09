using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record PricingLineInput(
    PassengerTypeCode? PassengerTypeCode,
    int? AgeFromInclusive,
    int? AgeToExclusive,
    AncillaryPriceLineCategory Category,
    string? Code,
    string? Name,
    int? CountryId,
    int? StationAirportId,
    decimal Amount);

public static class FiledRates
{
    public static IReadOnlyList<PricingRateInput> Of(IReadOnlyList<PricingLineInput> lines, int currencyId, FeeApplicationUnit? feeApplicationUnit)
        => lines
            .GroupBy(line => (line.PassengerTypeCode, line.AgeFromInclusive, line.AgeToExclusive))
            .SelectMany(selector =>
            {
                var components = selector
                    .Where(line => line.Category != AncillaryPriceLineCategory.Ancillary)
                    .Select(line => new PriceComponentInput(
                        line.Category,
                        line.Code!,
                        line.Name,
                        line.CountryId,
                        line.StationAirportId,
                        new MoneyInput(line.Amount, currencyId),
                        line.Category == AncillaryPriceLineCategory.Fee ? feeApplicationUnit : null,
                        null,
                        line.Category == AncillaryPriceLineCategory.Tax ? TaxTreatment.AddedToBase : null))
                    .ToList();
                var bases = selector.Where(line => line.Category == AncillaryPriceLineCategory.Ancillary).ToList();

                if (bases.Count == 0)
                    throw new InvalidOperationException("A tax or fee without a base price cannot be filed as a rate.");

                return bases.Select(line => new PricingRateInput(
                    selector.Key.PassengerTypeCode,
                    selector.Key.AgeFromInclusive,
                    selector.Key.AgeToExclusive,
                    new MoneyInput(line.Amount, currencyId),
                    components));
            })
            .ToList();
}
