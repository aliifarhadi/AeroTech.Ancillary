using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated
{
    public sealed class GetProvisionPricingsPaginatedService : IGetProvisionPricingsPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetProvisionPricingsPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<ProvisionPricingRowDto>> ExecuteAsync(IProvisionPricingsPaginatedQuery query, CancellationToken cancellationToken = default)
        {
            if (!await _dbContext.AncillaryProvisions.AsNoTracking().AnyAsync(provision => provision.Id == query.AncillaryProvisionId, cancellationToken))
                throw ExceptionFactory.ProvisionNotFound();

            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var rows = _dbContext.AncillaryPricings.AsNoTracking().Where(pricing => pricing.AncillaryProvisionId == query.AncillaryProvisionId);
            var totalCount = await rows.LongCountAsync(cancellationToken);
            var page = await rows
                .OrderByDescending(pricing => pricing.Version)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            var pricingIds = page.Select(pricing => pricing.Id).ToList();
            var rates = (await _dbContext.AncillaryPricingRates.AsNoTracking()
                    .Where(rate => pricingIds.Contains(rate.AncillaryPricingId))
                    .ToListAsync(cancellationToken))
                .ToLookup(rate => rate.AncillaryPricingId);
            var components = (await _dbContext.AncillaryPriceComponents.AsNoTracking()
                    .Where(component => pricingIds.Contains(component.AncillaryPricingId))
                    .ToListAsync(cancellationToken))
                .ToLookup(component => component.AncillaryPricingId);
            var currencyIds = rates.SelectMany(pricing => pricing).Select(rate => rate.CurrencyId).Distinct().ToList();
            var currencies = await _dbContext.Currencies.AsNoTracking()
                .Where(currency => currencyIds.Contains(currency.Id))
                .ToDictionaryAsync(currency => currency.Id, currency => currency.Code, cancellationToken);
            var projected = page.Select(pricing => AncillaryPricingMapper.ToProvisionPricingRow(
                AncillaryPricingMapper.ToBackofficePricing(pricing, rates[pricing.Id].ToList(), components[pricing.Id].ToList(), currencies)));

            return GridData<ProvisionPricingRowDto>.Create(
                PaginatedList<ProvisionPricingRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
