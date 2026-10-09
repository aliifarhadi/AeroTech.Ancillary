using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public sealed record FiledLine(
    PassengerTypeCode? PassengerTypeCode,
    int? AgeFromInclusive,
    int? AgeToExclusive,
    AncillaryPriceLineCategory Category,
    string? Code,
    string? Name,
    int? CountryId,
    int? StationAirportId,
    decimal Amount);

public static class FiledPrice
{
    public static readonly IReadOnlyDictionary<int, int> Scales = new Dictionary<int, int> { [47] = 2, [53] = 2, [70] = 0, [75] = 0, [82] = 3, [155] = 2 };

    public static IReadOnlyList<AncillaryPricingRateArgs> ToRates(IReadOnlyList<FiledLine> lines, int currencyId, FeeApplicationUnit? feeApplicationUnit)
        => lines
            .GroupBy(line => (line.PassengerTypeCode, line.AgeFromInclusive, line.AgeToExclusive))
            .SelectMany(selector =>
            {
                var components = selector
                    .Where(line => line.Category != AncillaryPriceLineCategory.Ancillary)
                    .Select(line => new AncillaryPriceComponentArgs(
                        line.Category,
                        line.Code!,
                        line.Name,
                        line.CountryId,
                        line.StationAirportId,
                        line.Amount,
                        currencyId,
                        line.Category == AncillaryPriceLineCategory.Fee ? feeApplicationUnit : null,
                        null,
                        line.Category == AncillaryPriceLineCategory.Tax ? TaxTreatment.AddedToBase : null))
                    .ToList();
                var bases = selector.Where(line => line.Category == AncillaryPriceLineCategory.Ancillary).ToList();

                if (bases.Count == 0)
                    throw new InvalidOperationException("A tax or fee without a base price cannot be filed as a rate.");

                return bases.Select(line => new AncillaryPricingRateArgs(
                    selector.Key.PassengerTypeCode,
                    selector.Key.AgeFromInclusive,
                    selector.Key.AgeToExclusive,
                    line.Amount,
                    currencyId,
                    components));
            })
            .ToList();
}
