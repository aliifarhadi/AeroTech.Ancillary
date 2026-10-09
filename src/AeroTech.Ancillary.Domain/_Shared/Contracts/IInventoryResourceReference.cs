using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IInventoryResourceReference
    {
        Task<InventoryReferenceCheck> CheckAsync(
            int ownerAirlineId,
            InventoryResourceKind resourceKind,
            long resourceId,
            CancellationToken cancellationToken = default);
    }
}
