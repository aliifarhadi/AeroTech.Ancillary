using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Messages.AirPrice.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.ServiceHost.ReferenceData
{
    public sealed class ReferenceDataRateOfExchangeReference : IRateOfExchangeReference
    {
        private readonly ReferenceDbContext _referenceData;

        public ReferenceDataRateOfExchangeReference(ReferenceDbContext referenceData) => _referenceData = referenceData;

        public async Task<IReadOnlyList<PublishedRateOfExchange>> FindRatesAsync(int toCurrencyId, DateTimeOffset at, CancellationToken cancellationToken = default)
        {
            var target = await _referenceData.Currencies.AsNoTracking().FirstOrDefaultAsync(currency => currency.Id == toCurrencyId, cancellationToken);

            if (target is null)
                return [];

            var dayStart = new DateTimeOffset(at.Year, at.Month, at.Day, 0, 0, 0, at.Offset);
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);
            var published = await _referenceData.RatesOfExchange.AsNoTracking()
                .Where(rate => rate.ToCurrencyId == toCurrencyId
                               && rate.Application == (int)RateApplication.FareConstruction
                               && rate.EffectiveFrom <= dayEnd
                               && (rate.EffectiveTo == null || rate.EffectiveTo >= dayStart))
                .OrderByDescending(rate => rate.EffectiveFrom)
                .ThenByDescending(rate => rate.Id)
                .ToListAsync(cancellationToken);

            return published
                .GroupBy(rate => rate.FromCurrencyId)
                .Select(group => group.First())
                .Select(rate => new PublishedRateOfExchange(rate.RateOfExchangeTableId, rate.FromCurrencyId, toCurrencyId, rate.Rate, target.DecimalPlaces, target.RoundingFactor))
                .Append(new PublishedRateOfExchange(target.Id.ToString(), toCurrencyId, toCurrencyId, 1m, target.DecimalPlaces, target.RoundingFactor))
                .ToList();
        }
    }
}
