using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Providers.InventoryReferences
{
    public sealed class NotConnectedFlightFlowDelegationReference : IFlightFlowDelegationReference
    {
        public Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, string providerKey, CancellationToken cancellationToken = default)
            => Task.FromResult(InventoryReferenceCheck.SourceUnavailable);
    }
}
