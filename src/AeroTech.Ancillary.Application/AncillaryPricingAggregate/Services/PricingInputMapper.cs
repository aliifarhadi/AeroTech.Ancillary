using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services
{
    internal static class PricingInputMapper
    {
        public static IReadOnlyList<AncillaryPricingLineArgs> ToArgs(this IReadOnlyList<PricingLineInput>? priceLines)
            => priceLines?
                .Select(line => new AncillaryPricingLineArgs(
                    line.PassengerTypeCode,
                    line.AgeFromInclusive,
                    line.AgeToExclusive,
                    line.Category,
                    line.Code,
                    line.Name,
                    line.CountryId,
                    line.StationAirportId,
                    line.Amount))
                .ToList() ?? [];
    }
}
