using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Providers.InventoryReferences
{
    public sealed class NotConnectedInventoryResourceReference : IInventoryResourceReference
    {
        public Task<InventoryReferenceCheck> CheckAsync(
            int ownerAirlineId,
            InventoryResourceKind resourceKind,
            long resourceId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(InventoryReferenceCheck.SourceUnavailable);
    }
}
