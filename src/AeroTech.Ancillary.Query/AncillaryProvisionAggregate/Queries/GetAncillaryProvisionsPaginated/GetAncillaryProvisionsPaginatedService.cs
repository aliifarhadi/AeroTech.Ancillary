using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
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
                .Select(provision => new
                {
                    Provision = provision,
                    TravelDates = _dbContext.AncillaryProvisionTravelDates.Count(row => row.AncillaryProvisionId == provision.Id),
                    SeasonalPeriods = _dbContext.AncillaryProvisionSeasonalPeriods.Count(row => row.AncillaryProvisionId == provision.Id),
                    BlackoutPeriods = _dbContext.AncillaryProvisionBlackoutPeriods.Count(row => row.AncillaryProvisionId == provision.Id),
                    DayTimeRestrictions = _dbContext.AncillaryProvisionDayTimeRestrictions.Count(row => row.AncillaryProvisionId == provision.Id)
                })
                .ToListAsync(cancellationToken);

            var projected = page.Select(row => AncillaryProvisionMapper.ToPaginatedRow(
                row.Provision,
                row.TravelDates,
                row.SeasonalPeriods,
                row.BlackoutPeriods,
                row.DayTimeRestrictions));

            return GridData<ProvisionPaginatedRowDto>.Create(
                PaginatedList<ProvisionPaginatedRowDto>.Create(projected, pageNumber, pageSize, totalCount));
        }
    }
}
