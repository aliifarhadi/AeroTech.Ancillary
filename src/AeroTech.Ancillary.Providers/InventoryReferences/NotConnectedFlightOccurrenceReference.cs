using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Providers.InventoryReferences
{
    public sealed class NotConnectedFlightOccurrenceReference : IFlightOccurrenceReference
    {
        public Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, long flightId, CancellationToken cancellationToken = default)
            => Task.FromResult(InventoryReferenceCheck.SourceUnavailable);
    }
}
