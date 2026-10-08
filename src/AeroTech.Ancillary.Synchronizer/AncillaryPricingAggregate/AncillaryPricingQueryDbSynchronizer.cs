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
            pricing.CurrencyId = snapshot.CurrencyId;
            pricing.FeeApplicationUnit = snapshot.FeeApplicationUnit;
            pricing.Status = snapshot.Status;
            pricing.CreatedAt = snapshot.CreatedAt;
            pricing.ActivatedAt = snapshot.ActivatedAt;
            pricing.SuspendedAt = snapshot.SuspendedAt;
            pricing.RetiredAt = snapshot.RetiredAt;
            pricing.LastUpdateTime = _clock.GetDateTime();

            var storedLines = await _dbContext.AncillaryPricingLines
                .Where(line => line.AncillaryPricingId == snapshot.PricingId)
                .ToListAsync(cancellationToken);

            _dbContext.AncillaryPricingLines.RemoveRange(
                storedLines.Where(line => snapshot.PriceLines.All(snapshotLine => snapshotLine.PriceLineId != line.Id)));

            foreach (var snapshotLine in snapshot.PriceLines)
            {
                var line = storedLines.FirstOrDefault(stored => stored.Id == snapshotLine.PriceLineId);

                if (line is null)
                {
                    line = new AncillaryPricingLineReadModel { Id = snapshotLine.PriceLineId };
                    _dbContext.AncillaryPricingLines.Add(line);
                }

                line.AncillaryPricingId = snapshot.PricingId;
                line.PassengerTypeCode = snapshotLine.PassengerTypeCode;
                line.AgeFromInclusive = snapshotLine.AgeFromInclusive;
                line.AgeToExclusive = snapshotLine.AgeToExclusive;
                line.Category = snapshotLine.Category;
                line.Code = snapshotLine.Code;
                line.Name = snapshotLine.Name;
                line.CountryId = snapshotLine.CountryId;
                line.StationAirportId = snapshotLine.StationAirportId;
                line.Amount = snapshotLine.Amount;
            }
        }
    }
}
