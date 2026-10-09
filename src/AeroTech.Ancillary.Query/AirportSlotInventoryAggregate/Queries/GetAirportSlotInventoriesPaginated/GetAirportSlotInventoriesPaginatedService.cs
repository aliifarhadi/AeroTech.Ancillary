using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated
{
    public sealed class GetAirportSlotInventoriesPaginatedService : IGetAirportSlotInventoriesPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetAirportSlotInventoriesPaginatedService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<GridData<AirportSlotInventoryPaginatedRowDto>> ExecuteAsync(IGetAirportSlotInventoriesPaginatedQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var rows = _dbContext.AirportSlotInventories
                .AsNoTracking()
                .Where(inventory => inventory.OwnerAirlineId == ownerAirlineId
                                    && (query.AirportId == null || inventory.AirportId == query.AirportId)
                                    && (query.FacilityId == null || inventory.FacilityId == query.FacilityId)
                                    && (query.FromUtc == null || inventory.EndUtc > query.FromUtc)
                                    && (query.ToUtc == null || inventory.StartUtc < query.ToUtc)
                                    && (query.Status == null || inventory.Status == query.Status)
                                    && (query.ClosedForSale == null || inventory.ClosedForSale == query.ClosedForSale));
            var totalCount = await rows.LongCountAsync(cancellationToken);
            var page = await rows
                .OrderBy(inventory => inventory.FacilityId)
                .ThenBy(inventory => inventory.StartUtc)
                .ThenBy(inventory => inventory.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GridData<AirportSlotInventoryPaginatedRowDto>.Create(
                PaginatedList<AirportSlotInventoryPaginatedRowDto>.Create(page.Select(AirportSlotInventoryMapper.ToPaginatedRow), pageNumber, pageSize, totalCount));
        }
    }
}
