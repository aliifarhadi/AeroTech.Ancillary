using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById
{
    public sealed class GetAncillaryPriceRuleByIdService : IGetAncillaryPriceRuleByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryPriceRuleByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficeAncillaryPriceRuleDto> ExecuteAsync(long ancillaryPriceRuleId, CancellationToken cancellationToken = default)
        {
            var rule = await _dbContext.AncillaryPriceRules
                           .AsNoTracking()
                           .FirstOrDefaultAsync(row => row.Id == ancillaryPriceRuleId, cancellationToken)
                       ?? throw ExceptionFactory.AncillaryPriceRuleNotFound();

            var lines = await _dbContext.PriceLines
                .AsNoTracking()
                .Where(line => line.AncillaryPriceRuleId == rule.Id)
                .ToListAsync(cancellationToken);

            return AncillaryPriceRuleMapper.ToBackofficePriceRule(rule, lines);
        }
    }
}
