using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Services
{
    internal static class ProvisionCurrencyScale
    {
        private static readonly IReadOnlyDictionary<int, int> NoCurrencies = new Dictionary<int, int>();

        public static async Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(
            this ICurrencyReference currencies,
            ProvisionSettlementInput settlement,
            CancellationToken cancellationToken)
        {
            var currencyIds = settlement.CancellationRules?.Select(rule => rule.PenaltyCurrencyId).OfType<int>().Distinct().ToList() ?? [];

            return currencyIds.Count == 0
                ? NoCurrencies
                : await currencies.FindDecimalPlacesAsync(currencyIds, cancellationToken);
        }
    }
}
