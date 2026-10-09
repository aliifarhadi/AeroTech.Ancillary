using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record InventoryPolicyEvidence(
        bool ServiceDefinitionMatchesIdentity,
        PricingUnit? PricingUnit,
        bool ActiveProvisionMustCheckAvailability,
        string? SupplierProviderKey,
        InventoryReferenceCheck FlightFlowDelegation,
        InventoryReferenceCheck CountResource,
        InventoryCountUnit? CountUnitOfLiveSources,
        InventoryReferenceCheck WeightResource,
        InventoryReferenceCheck SlotFacility,
        IReadOnlyDictionary<string, InventoryReferenceCheck> CountingFamilies);
}
