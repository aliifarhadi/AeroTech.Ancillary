namespace AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts
{
    public interface IAirportSlotInventoryRepository
    {
        Task AddAsync(AirportSlotInventory inventory, CancellationToken cancellationToken = default);

        Task<AirportSlotInventory?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<IAsyncDisposable> LockFacilityAsync(int ownerAirlineId, long facilityId, CancellationToken cancellationToken = default);

        Task<bool> HasOverlapAsync(
            int ownerAirlineId,
            long facilityId,
            DateTimeOffset startUtc,
            DateTimeOffset endUtc,
            CancellationToken cancellationToken = default);
    }
}
