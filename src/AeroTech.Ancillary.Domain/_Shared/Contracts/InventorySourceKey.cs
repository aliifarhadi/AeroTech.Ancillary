using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record InventorySourceKey(InventoryResourceKind ResourceKind, long InventorySourceId);
}
