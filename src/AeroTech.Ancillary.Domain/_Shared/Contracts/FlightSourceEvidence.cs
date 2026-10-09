using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record FlightSourceEvidence(
        InventoryReferenceCheck Flight,
        InventoryReferenceCheck Resource,
        InventoryCountUnit? CountUnitOfLiveSources);
}
