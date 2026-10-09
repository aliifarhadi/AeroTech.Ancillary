using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;
using AeroTech.Framework.Core.Domain.Queries;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated
{
    public interface IGetAirportSlotInventoriesPaginatedService
    {
        Task<GridData<AirportSlotInventoryPaginatedRowDto>> ExecuteAsync(IGetAirportSlotInventoriesPaginatedQuery query, CancellationToken cancellationToken = default);
    }
}
