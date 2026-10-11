namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IFlightSeatMapReference
    {
        Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, long flightId, CancellationToken cancellationToken = default);
    }
}
