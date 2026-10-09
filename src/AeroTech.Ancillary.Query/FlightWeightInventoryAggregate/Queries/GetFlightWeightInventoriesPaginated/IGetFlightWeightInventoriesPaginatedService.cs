using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated
{
    public interface IGetFlightWeightInventoriesPaginatedService
    {
        Task<GridData<FlightWeightInventoryPaginatedRowDto>> ExecuteAsync(IGetFlightWeightInventoriesPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
