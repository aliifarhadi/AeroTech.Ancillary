using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryPriceRuleAggregate
{
    public sealed class AncillaryPriceRuleRepository : IAncillaryPriceRuleRepository
    {
        private readonly AncillaryDbContext _dbContext;

        public AncillaryPriceRuleRepository(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task AddAsync(AncillaryPriceRule rule, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryPriceRules.AddAsync(rule, cancellationToken);

        public Task<AncillaryPriceRule?> GetAsync(long id, CancellationToken cancellationToken = default)
            => _dbContext.AncillaryPriceRules
                .Include(rule => rule.Lines)
                .FirstOrDefaultAsync(rule => rule.Id == id, cancellationToken);

        public Task<bool> ActivePriorityExistsAsync(
            int ownerAirlineId,
            string productRef,
            int currencyId,
            int priority,
            long exceptRuleId,
            CancellationToken cancellationToken = default)
            => _dbContext.AncillaryPriceRules
                .AnyAsync(rule => rule.Id != exceptRuleId
                                  && rule.OwnerAirlineId == ownerAirlineId
                                  && rule.ProductRef == productRef
                                  && rule.CurrencyId == currencyId
                                  && rule.Priority == priority
                                  && rule.Status == AncillaryPriceRuleStatus.Active,
                    cancellationToken);

        public async Task<IReadOnlyList<AncillaryPriceRule>> ListActiveAsync(int currencyId, CancellationToken cancellationToken = default)
            => await _dbContext.AncillaryPriceRules
                .AsNoTracking()
                .Include(rule => rule.Lines)
                .Where(rule => rule.CurrencyId == currencyId && rule.Status == AncillaryPriceRuleStatus.Active)
                .ToListAsync(cancellationToken);
    }
}
