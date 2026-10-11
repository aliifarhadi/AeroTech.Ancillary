using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Providers.SeatMap
{
    public sealed class NotConnectedFlightSeatMapReference : IFlightSeatMapReference
    {
        public Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, long flightId, CancellationToken cancellationToken = default)
            => Task.FromResult(InventoryReferenceCheck.SourceUnavailable);
    }
}
