using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated
{
    public sealed class GetAncillaryPriceRulesPaginatedService : IGetAncillaryPriceRulesPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryPriceRulesPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<AncillaryPriceRulePaginatedRowDto>> ExecuteAsync(
            IAncillaryPriceRulesPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var rows = _dbContext.AncillaryPriceRules
                .AsNoTracking()
                .Where(rule => (query.OwnerAirlineId == null || rule.OwnerAirlineId == query.OwnerAirlineId)
                               && (query.ProductRef == null || rule.ProductRef == query.ProductRef)
                               && (query.CurrencyId == null || rule.CurrencyId == query.CurrencyId)
                               && (query.Status == null || rule.Status == query.Status));

            var totalCount = await rows.LongCountAsync(cancellationToken);

            var page = await rows
                .OrderBy(rule => rule.ProductRef)
                .ThenBy(rule => rule.CurrencyId)
                .ThenBy(rule => rule.Priority)
                .ThenBy(rule => rule.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var ruleIds = page.Select(rule => rule.Id).ToList();

            var linesByRule = (await _dbContext.PriceLines.AsNoTracking()
                    .Where(line => ruleIds.Contains(line.AncillaryPriceRuleId))
                    .ToListAsync(cancellationToken))
                .ToLookup(line => line.AncillaryPriceRuleId);

            var projected = page.Select(rule => AncillaryPriceRuleMapper.ToPaginatedRow(rule, linesByRule[rule.Id]));

            return GridData<AncillaryPriceRulePaginatedRowDto>.Create(
                PaginatedList<AncillaryPriceRulePaginatedRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
