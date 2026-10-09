using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated
{
    public interface IGetFlightCountInventoriesPaginatedService
    {
        Task<GridData<FlightCountInventoryPaginatedRowDto>> ExecuteAsync(IGetFlightCountInventoriesPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
