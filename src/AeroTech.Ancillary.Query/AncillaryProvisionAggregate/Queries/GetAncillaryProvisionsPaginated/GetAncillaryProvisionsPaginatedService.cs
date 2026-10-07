using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated
{
    public sealed class GetAncillaryProvisionsPaginatedService : IGetAncillaryProvisionsPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;

        public GetAncillaryProvisionsPaginatedService(AncillaryQueryDbContext dbContext) => _dbContext = dbContext;

        public async Task<GridData<ProvisionPaginatedRowDto>> ExecuteAsync(
            IAncillaryProvisionsPaginatedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var rows = from provision in _dbContext.AncillaryProvisions.AsNoTracking()
                       join definition in _dbContext.AncillaryServiceDefinitions on provision.ServiceDefinitionId equals definition.Id
                       where provision.ServiceDefinitionId == query.ServiceDefinitionId
                             && (query.SupplierId == null || definition.SupplierId == query.SupplierId)
                             && (query.Status == null || provision.Status == query.Status)
                             && (query.CoverageScope == null || provision.CoverageScope == query.CoverageScope)
                             && (query.Sequence == null || provision.Sequence == query.Sequence)
                             && (query.SalesDate == null
                                 || ((provision.SalesEffectiveFrom == null || provision.SalesEffectiveFrom <= query.SalesDate)
                                     && (provision.SalesDiscontinueAt == null || query.SalesDate < provision.SalesDiscontinueAt)))
                       select provision;

            var totalCount = await rows.LongCountAsync(cancellationToken);

            var page = await rows
                .OrderBy(provision => provision.Sequence)
                .ThenByDescending(provision => provision.CreatedAt)
                .ThenBy(provision => provision.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var provisionIds = page.Select(provision => provision.Id).ToList();

            var linesByProvision = (await _dbContext.AncillaryProvisionPriceLines.AsNoTracking()
                    .Where(line => provisionIds.Contains(line.AncillaryProvisionId))
                    .ToListAsync(cancellationToken))
                .ToLookup(line => line.AncillaryProvisionId);

            var currencyIds = page
                .Where(provision => provision.FeeCurrencyId.HasValue)
                .Select(provision => provision.FeeCurrencyId!.Value)
                .Distinct()
                .ToList();

            var currencies = await _dbContext.Currencies.AsNoTracking()
                .Where(currency => currencyIds.Contains(currency.Id))
                .ToDictionaryAsync(currency => currency.Id, currency => currency.Code, cancellationToken);

            var projected = page.Select(provision => AncillaryProvisionMapper.ToPaginatedRow(
                provision,
                linesByProvision[provision.Id].ToList(),
                provision.FeeCurrencyId is { } currencyId ? currencies.GetValueOrDefault(currencyId) : null));

            return GridData<ProvisionPaginatedRowDto>.Create(
                PaginatedList<ProvisionPaginatedRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
