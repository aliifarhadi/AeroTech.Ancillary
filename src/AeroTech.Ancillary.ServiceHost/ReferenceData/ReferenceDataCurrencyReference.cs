using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataCurrencyReference : ICurrencyReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataCurrencyReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public async Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default)
            => await _referenceData.Currencies.AsNoTracking()
                .Where(currency => currencyIds.Contains(currency.Id))
                .ToDictionaryAsync(currency => currency.Id, currency => currency.DecimalPlaces, cancellationToken);
    }
}
