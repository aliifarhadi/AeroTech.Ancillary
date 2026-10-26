using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record InventoryCommercialFacts(
        long ServiceDefinitionId,
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        AncillaryProfile? Profile,
        PricingUnit? PricingUnit,
        string? ActiveExternalSupplierProviderKey,
        bool? RequiresSpecificAppointment);

    public interface IInventoryCommercialFactsReader
    {
        Task<InventoryCommercialFacts?> FindAsync(long serviceDefinitionId, CancellationToken cancellationToken = default);

        Task<InventoryCommercialFacts?> FindCurrentAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default);
    }
}
