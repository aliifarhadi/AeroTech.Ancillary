using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById
{
    public sealed class GetAncillaryPricingByIdService : IGetAncillaryPricingByIdService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryPricingByIdService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<BackofficePricingDto> ExecuteAsync(long pricingId, CancellationToken cancellationToken = default)
        {
            var pricing = await _dbContext.AncillaryPricings
                              .AsNoTracking()
                              .FirstOrDefaultAsync(row => row.Id == pricingId, cancellationToken)
                          ?? throw ExceptionFactory.PricingNotFound();
            var priceLines = await _dbContext.AncillaryPricingLines
                .AsNoTracking()
                .Where(line => line.AncillaryPricingId == pricingId)
                .ToListAsync(cancellationToken);
            var currency = await _dbContext.Currencies
                .AsNoTracking()
                .Where(row => row.Id == pricing.CurrencyId)
                .Select(row => row.Code)
                .FirstOrDefaultAsync(cancellationToken);

            return AncillaryPricingMapper.ToBackofficePricing(pricing, priceLines, currency);
        }
    }
}
