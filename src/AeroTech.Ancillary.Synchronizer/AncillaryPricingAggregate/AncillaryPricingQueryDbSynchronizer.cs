using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryPricingAggregate
{
    public sealed class AncillaryPricingQueryDbSynchronizer : IAncillaryPricingQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public AncillaryPricingQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(AncillaryPricingReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var pricing = await _dbContext.AncillaryPricings
                .FirstOrDefaultAsync(row => row.Id == snapshot.PricingId, cancellationToken);

            if (pricing is null)
            {
                pricing = new AncillaryPricingReadModel { Id = snapshot.PricingId };
                _dbContext.AncillaryPricings.Add(pricing);
            }

            pricing.AncillaryProvisionId = snapshot.AncillaryProvisionId;
            pricing.PricingUnit = snapshot.PricingUnit;
            pricing.Version = snapshot.Version;
            pricing.Status = snapshot.Status;
            pricing.CreatedAt = snapshot.CreatedAt;
            pricing.ActivatedAt = snapshot.ActivatedAt;
            pricing.SuspendedAt = snapshot.SuspendedAt;
            pricing.RetiredAt = snapshot.RetiredAt;
            pricing.LastUpdateTime = _clock.GetDateTime();

            var storedRates = await _dbContext.AncillaryPricingRates
                .Where(rate => rate.AncillaryPricingId == snapshot.PricingId)
                .ToListAsync(cancellationToken);
            var storedComponents = await _dbContext.AncillaryPriceComponents
                .Where(component => component.AncillaryPricingId == snapshot.PricingId)
                .ToListAsync(cancellationToken);
            var snapshotComponents = snapshot.Rates
                .SelectMany(rate => rate.Components.Select(component => (RateId: rate.RateId, Component: component)))
                .ToList();

            _dbContext.AncillaryPricingRates.RemoveRange(
                storedRates.Where(rate => snapshot.Rates.All(snapshotRate => snapshotRate.RateId != rate.Id)));
            _dbContext.AncillaryPriceComponents.RemoveRange(
                storedComponents.Where(component => snapshotComponents.All(snapshotComponent => snapshotComponent.Component.ComponentId != component.Id)));

            foreach (var snapshotRate in snapshot.Rates)
            {
                var rate = storedRates.FirstOrDefault(stored => stored.Id == snapshotRate.RateId);

                if (rate is null)
                {
                    rate = new AncillaryPricingRateReadModel { Id = snapshotRate.RateId };
                    _dbContext.AncillaryPricingRates.Add(rate);
                }

                rate.AncillaryPricingId = snapshot.PricingId;
                rate.PassengerTypeCode = snapshotRate.PassengerTypeCode;
                rate.AgeFromInclusive = snapshotRate.AgeFromInclusive;
                rate.AgeToExclusive = snapshotRate.AgeToExclusive;
                rate.CurrencyId = snapshotRate.CurrencyId;
                rate.BaseAmount = snapshotRate.BaseAmount;
            }

            foreach (var (rateId, snapshotComponent) in snapshotComponents)
            {
                var component = storedComponents.FirstOrDefault(stored => stored.Id == snapshotComponent.ComponentId);

                if (component is null)
                {
                    component = new AncillaryPriceComponentReadModel { Id = snapshotComponent.ComponentId };
                    _dbContext.AncillaryPriceComponents.Add(component);
                }

                component.AncillaryPricingId = snapshot.PricingId;
                component.AncillaryPricingRateId = rateId;
                component.Category = snapshotComponent.Category;
                component.Code = snapshotComponent.Code;
                component.Name = snapshotComponent.Name;
                component.CountryId = snapshotComponent.CountryId;
                component.StationAirportId = snapshotComponent.StationAirportId;
                component.Amount = snapshotComponent.Amount;
                component.CurrencyId = snapshotComponent.CurrencyId;
                component.FeeApplicationUnit = snapshotComponent.FeeApplicationUnit;
                component.TaxIncludedInSource = snapshotComponent.TaxIncludedInSource;
            }
        }
    }
}
