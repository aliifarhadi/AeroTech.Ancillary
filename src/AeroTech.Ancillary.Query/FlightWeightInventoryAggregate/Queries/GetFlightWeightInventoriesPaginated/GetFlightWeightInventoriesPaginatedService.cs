using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated
{
    public sealed class GetFlightWeightInventoriesPaginatedService : IGetFlightWeightInventoriesPaginatedService
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IInventoryCallerScope _scope;

        public GetFlightWeightInventoriesPaginatedService(AncillaryQueryDbContext dbContext, IInventoryCallerScope scope)
        {
            _dbContext = dbContext;
            _scope = scope;
        }

        public async Task<GridData<FlightWeightInventoryPaginatedRowDto>> ExecuteAsync(IGetFlightWeightInventoriesPaginatedQuery query, CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            var ownerAirlineId = await _scope.RequireOwnerAirlineIdAsync(cancellationToken);
            var rows = _dbContext.FlightWeightInventories
                .AsNoTracking()
                .Where(inventory => inventory.OwnerAirlineId == ownerAirlineId
                                    && (query.FlightId == null || inventory.FlightId == query.FlightId)
                                    && (query.WeightResourceId == null || inventory.WeightResourceId == query.WeightResourceId)
                                    && (query.Status == null || inventory.Status == query.Status)
                                    && (query.ClosedForSale == null || inventory.ClosedForSale == query.ClosedForSale));
            var totalCount = await rows.LongCountAsync(cancellationToken);
            var page = await rows
                .OrderBy(inventory => inventory.FlightId)
                .ThenBy(inventory => inventory.WeightResourceId)
                .ThenBy(inventory => inventory.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return GridData<FlightWeightInventoryPaginatedRowDto>.Create(
                PaginatedList<FlightWeightInventoryPaginatedRowDto>.Create(page.Select(FlightWeightInventoryMapper.ToPaginatedRow), pageNumber, pageSize, totalCount));
        }
    }
}
