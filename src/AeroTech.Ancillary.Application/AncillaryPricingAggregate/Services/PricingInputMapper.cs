using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services
{
    internal static class PricingInputMapper
    {
        public static IReadOnlyList<AncillaryPricingRateArgs> ToArgs(this IReadOnlyList<PricingRateInput>? rates)
            => rates?
                .Select(rate => new AncillaryPricingRateArgs(
                    rate.PassengerTypeCode,
                    rate.AgeFromInclusive,
                    rate.AgeToExclusive,
                    rate.BasePrice.Amount,
                    rate.BasePrice.CurrencyId,
                    (rate.Components ?? [])
                        .Select(component => new AncillaryPriceComponentArgs(
                            component.Category,
                            component.Code,
                            component.Name,
                            component.CountryId,
                            component.StationAirportId,
                            component.Amount.Amount,
                            component.Amount.CurrencyId,
                            component.FeeApplicationUnit,
                            component.TaxIncludedInSource))
                        .ToList()))
                .ToList() ?? [];
    }
}
