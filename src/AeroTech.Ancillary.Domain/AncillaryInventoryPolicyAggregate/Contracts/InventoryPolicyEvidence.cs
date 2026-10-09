using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record InventoryPolicyEvidence(
        long? CurrentServiceDefinitionId,
        PricingUnit? PricingUnit,
        string? SupplierProviderKey,
        InventoryReferenceCheck FlightFlowDelegation,
        InventoryReferenceCheck CountResource,
        InventoryCountUnit? CountUnitOfLiveSources,
        InventoryReferenceCheck WeightResource,
        InventoryReferenceCheck SlotFacility,
        IReadOnlyDictionary<string, InventoryReferenceCheck> CountingFamilies,
        IReadOnlyDictionary<string, IReadOnlyList<UsageConsumptionUnit>>? CountingFamilyUnits = null);
}
