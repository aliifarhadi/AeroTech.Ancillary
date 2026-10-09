using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Dto;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById
{
    public interface IGetFlightWeightInventoryByIdService
    {
        Task<BackofficeFlightWeightInventoryDto> ExecuteAsync(long inventoryId, CancellationToken cancellationToken = default);
    }
}
