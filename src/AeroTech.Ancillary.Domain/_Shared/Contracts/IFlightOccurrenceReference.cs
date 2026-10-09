namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IFlightOccurrenceReference
    {
        Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, long flightId, CancellationToken cancellationToken = default);
    }
}
