using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated
{
    public sealed class GetFlightCountInventoriesPaginatedService : IGetFlightCountInventoriesPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetFlightCountInventoriesPaginatedService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<GridData<FlightCountInventoryPaginatedRowDto>> ExecuteAsync(IGetFlightCountInventoriesPaginatedQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var rows = _dbContext.FlightCountInventories
                .AsNoTracking()
                .Where(inventory => inventory.OwnerAirlineId == ownerAirlineId
                                    && (query.FlightId == null || inventory.FlightId == query.FlightId)
                                    && (query.ResourceId == null || inventory.ResourceId == query.ResourceId)
                                    && (query.Status == null || inventory.Status == query.Status)
                                    && (query.ClosedForSale == null || inventory.ClosedForSale == query.ClosedForSale));
            var totalCount = await rows.LongCountAsync(cancellationToken);
            var page = await rows
                .OrderBy(inventory => inventory.FlightId)
                .ThenBy(inventory => inventory.ResourceId)
                .ThenBy(inventory => inventory.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GridData<FlightCountInventoryPaginatedRowDto>.Create(
                PaginatedList<FlightCountInventoryPaginatedRowDto>.Create(page.Select(FlightCountInventoryMapper.ToPaginatedRow), pageNumber, pageSize, totalCount));
        }
    }
}
