using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Dto;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById
{
    public interface IGetAirportSlotInventoryByIdService
    {
        Task<BackofficeAirportSlotInventoryDto> ExecuteAsync(long inventoryId, CancellationToken cancellationToken = default);
    }
}
