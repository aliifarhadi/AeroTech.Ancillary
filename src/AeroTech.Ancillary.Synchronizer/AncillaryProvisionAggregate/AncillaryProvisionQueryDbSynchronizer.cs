using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryProvisionAggregate
{
    public sealed class AncillaryProvisionQueryDbSynchronizer : IAncillaryProvisionQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public AncillaryProvisionQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(AncillaryProvisionReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var provision = await _dbContext.AncillaryProvisions
                .FirstOrDefaultAsync(row => row.Id == snapshot.ProvisionId, cancellationToken);

            if (provision is null)
            {
                provision = new AncillaryProvisionReadModel { Id = snapshot.ProvisionId };
                _dbContext.AncillaryProvisions.Add(provision);
            }

            provision.ServiceDefinitionId = snapshot.ServiceDefinitionId;
            provision.Sequence = snapshot.Sequence;
            provision.Status = snapshot.Status;
            provision.SalesEffectiveFrom = snapshot.SalesEffectiveFrom;
            provision.SalesDiscontinueAt = snapshot.SalesDiscontinueAt;
            provision.CoverageScope = snapshot.CoverageScope;
            provision.QuantityUnit = snapshot.QuantityUnit;
            provision.MinQuantity = snapshot.MinQuantity;
            provision.MaxQuantity = snapshot.MaxQuantity;
            provision.ApplicationType = snapshot.ApplicationType;
            provision.Disposition = snapshot.Disposition;
            provision.DocumentRequired = snapshot.DocumentRequired;
            provision.BookingRequired = snapshot.BookingRequired;
            provision.FeeCurrencyId = snapshot.FeeCurrencyId;
            provision.FeeApplicationUnit = snapshot.FeeApplicationUnit;
            provision.ReissueRefund = snapshot.ReissueRefund;
            provision.FormOfRefund = snapshot.FormOfRefund;
            provision.Commissionable = snapshot.Commissionable;
            provision.InterlineSettlement = snapshot.InterlineSettlement;
            provision.MustCheckAvailability = snapshot.MustCheckAvailability;
            provision.FulfillmentProviderKey = snapshot.FulfillmentProviderKey;
            provision.CreatedAt = snapshot.CreatedAt;
            provision.LastUpdateTime = _clock.GetDateTime();

            var storedLines = await _dbContext.AncillaryProvisionPriceLines
                .Where(line => line.AncillaryProvisionId == snapshot.ProvisionId)
                .ToListAsync(cancellationToken);

            _dbContext.AncillaryProvisionPriceLines.RemoveRange(
                storedLines.Where(line => snapshot.PriceLines.All(snapshotLine => snapshotLine.PriceLineId != line.Id)));

            foreach (var snapshotLine in snapshot.PriceLines)
            {
                var line = storedLines.FirstOrDefault(stored => stored.Id == snapshotLine.PriceLineId);

                if (line is null)
                {
                    line = new AncillaryProvisionPriceLineReadModel { Id = snapshotLine.PriceLineId };
                    _dbContext.AncillaryProvisionPriceLines.Add(line);
                }

                line.AncillaryProvisionId = snapshot.ProvisionId;
                line.Category = snapshotLine.Category;
                line.Code = snapshotLine.Code;
                line.Name = snapshotLine.Name;
                line.UnitAmount = snapshotLine.UnitAmount;
            }
        }
    }
}
