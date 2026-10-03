using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.AncillaryPriceRuleAggregate
{
    public sealed class AncillaryPriceRuleQueryDbSynchronizer : IAncillaryPriceRuleQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public AncillaryPriceRuleQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(AncillaryPriceRuleReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var rule = await _dbContext.AncillaryPriceRules.FirstOrDefaultAsync(row => row.Id == snapshot.AncillaryPriceRuleId, cancellationToken);

            if (rule is null)
            {
                rule = new AncillaryPriceRuleReadModel { Id = snapshot.AncillaryPriceRuleId };
                _dbContext.AncillaryPriceRules.Add(rule);
            }

            rule.OwnerAirlineId = snapshot.OwnerAirlineId;
            rule.ProductRef = snapshot.ProductRef;
            rule.Priority = snapshot.Priority;
            rule.CurrencyId = snapshot.CurrencyId;
            rule.SalesFrom = snapshot.SalesFrom;
            rule.SalesTo = snapshot.SalesTo;
            rule.TravelFrom = snapshot.TravelFrom;
            rule.TravelTo = snapshot.TravelTo;
            rule.PassengerTypes = snapshot.PassengerTypes?.ToList();
            rule.OriginAirportIds = snapshot.OriginAirportIds?.ToList();
            rule.DestinationAirportIds = snapshot.DestinationAirportIds?.ToList();
            rule.Status = snapshot.Status;
            rule.CreatedAt = snapshot.CreatedAt;
            rule.LastUpdateTime = _clock.GetDateTime();

            await ReconcileLinesAsync(snapshot, cancellationToken);
        }

        private async Task ReconcileLinesAsync(AncillaryPriceRuleReadModelSnapshot snapshot, CancellationToken cancellationToken)
        {
            var stored = await _dbContext.PriceLines
                .Where(row => row.AncillaryPriceRuleId == snapshot.AncillaryPriceRuleId)
                .ToListAsync(cancellationToken);
            var storedById = stored.ToDictionary(row => row.Id);

            foreach (var line in snapshot.Lines)
            {
                if (!storedById.TryGetValue(line.PriceLineId, out var row))
                {
                    row = new PriceLineReadModel { Id = line.PriceLineId, AncillaryPriceRuleId = snapshot.AncillaryPriceRuleId };
                    _dbContext.PriceLines.Add(row);
                }

                row.Category = line.Category;
                row.Code = line.Code;
                row.Name = line.Name;
                row.Amount = line.Amount;
            }

            var incomingIds = snapshot.Lines.Select(line => line.PriceLineId).ToHashSet();

            foreach (var row in stored.Where(row => !incomingIds.Contains(row.Id)))
                _dbContext.PriceLines.Remove(row);
        }
    }
}
