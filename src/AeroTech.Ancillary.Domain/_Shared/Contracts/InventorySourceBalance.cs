using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record InventorySourceBalance(
        decimal Capacity,
        InventoryRecordStatus Status,
        bool ClosedForSale,
        decimal ActiveUsage);
}
