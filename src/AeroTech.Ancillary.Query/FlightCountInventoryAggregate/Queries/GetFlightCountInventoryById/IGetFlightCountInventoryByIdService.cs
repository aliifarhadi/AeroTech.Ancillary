using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Dto;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById
{
    public interface IGetFlightCountInventoryByIdService
    {
        Task<BackofficeFlightCountInventoryDto> ExecuteAsync(long inventoryId, CancellationToken cancellationToken = default);
    }
}
