using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Providers.InventoryReferences
{
    public sealed class NotConnectedCountingFamilyReference : ICountingFamilyReference
    {
        public Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, string countingFamilyCode, CancellationToken cancellationToken = default)
            => Task.FromResult(InventoryReferenceCheck.SourceUnavailable);
    }
}
