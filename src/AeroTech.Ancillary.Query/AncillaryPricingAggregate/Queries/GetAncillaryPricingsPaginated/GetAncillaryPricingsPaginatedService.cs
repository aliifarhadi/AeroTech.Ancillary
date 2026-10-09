using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated
{
    public sealed class GetAncillaryPricingsPaginatedService : IGetAncillaryPricingsPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryPricingsPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<PricingPaginatedRowDto>> ExecuteAsync(
            IAncillaryPricingsPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var rows = _dbContext.AncillaryPricings.AsNoTracking()
                .Where(pricing => (query.AncillaryProvisionId == null || pricing.AncillaryProvisionId == query.AncillaryProvisionId)
                                  && (query.Status == null || pricing.Status == query.Status));

            var totalCount = await rows.LongCountAsync(cancellationToken);
            var page = await rows
                .OrderBy(pricing => pricing.AncillaryProvisionId)
                .ThenByDescending(pricing => pricing.Version)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var pricingIds = page.Select(pricing => pricing.Id).ToList();
            var rates = (await _dbContext.AncillaryPricingRates.AsNoTracking()
                    .Where(rate => pricingIds.Contains(rate.AncillaryPricingId))
                    .ToListAsync(cancellationToken))
                .ToLookup(rate => rate.AncillaryPricingId);
            var currencyIds = rates.SelectMany(pricing => pricing).Select(rate => rate.CurrencyId).Distinct().ToList();
            var currencies = await _dbContext.Currencies.AsNoTracking()
                .Where(currency => currencyIds.Contains(currency.Id))
                .ToDictionaryAsync(currency => currency.Id, currency => currency.Code, cancellationToken);

            var projected = page.Select(pricing => AncillaryPricingMapper.ToPaginatedRow(pricing, rates[pricing.Id].ToList(), currencies));

            return GridData<PricingPaginatedRowDto>.Create(
                PaginatedList<PricingPaginatedRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
