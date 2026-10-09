using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services
{
    internal static class PricingCurrencyScale
    {
        public static Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(
            this ICurrencyReference currencies,
            IReadOnlyList<AncillaryPricingRateArgs> rates,
            CancellationToken cancellationToken)
            => currencies.FindDecimalPlacesAsync(
                rates.Select(rate => rate.CurrencyId)
                    .Concat(rates.SelectMany(rate => rate.Components ?? []).Select(component => component.CurrencyId))
                    .Distinct()
                    .ToList(),
                cancellationToken);

        public static Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(
            this ICurrencyReference currencies,
            AncillaryPricing pricing,
            CancellationToken cancellationToken)
            => currencies.FindDecimalPlacesAsync(pricing.Rates.Select(rate => rate.CurrencyId).Distinct().ToList(), cancellationToken);
    }
}
