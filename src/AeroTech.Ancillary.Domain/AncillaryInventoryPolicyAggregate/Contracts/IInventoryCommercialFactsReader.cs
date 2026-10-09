using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record InventoryCommercialFacts(
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        PricingUnit? PricingUnit,
        string? ActiveExternalSupplierProviderKey,
        bool ActiveProvisionMustCheckAvailability);

    public interface IInventoryCommercialFactsReader
    {
        Task<InventoryCommercialFacts?> FindAsync(long serviceDefinitionId, CancellationToken cancellationToken = default);
    }
}
