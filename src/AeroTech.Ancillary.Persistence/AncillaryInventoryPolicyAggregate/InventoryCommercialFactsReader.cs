using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate
{
    public sealed class InventoryCommercialFactsReader : IInventoryCommercialFactsReader
    {
        private readonly AncillaryDbContext _dbContext;

        public InventoryCommercialFactsReader(AncillaryDbContext dbContext) => _dbContext = dbContext;

        public async Task<InventoryCommercialFacts?> FindAsync(long serviceDefinitionId, CancellationToken cancellationToken = default)
        {
            var definition = await _dbContext.AncillaryServiceDefinitions
                .AsNoTracking()
                .Where(row => row.Id == serviceDefinitionId)
                .Select(row => new { row.OwnerAirlineId, row.ServiceDefinitionRef, row.PricingUnit, row.SupplierId })
                .FirstOrDefaultAsync(cancellationToken);

            if (definition is null)
                return null;

            var providerKey = await _dbContext.Suppliers
                .AsNoTracking()
                .Where(supplier => supplier.Id == definition.SupplierId
                                   && supplier.Status == SupplierStatus.Active
                                   && supplier.FulfillmentKind == SupplierFulfillmentKind.External)
                .Select(supplier => supplier.FulfillmentProviderKey)
                .FirstOrDefaultAsync(cancellationToken);
            var versions = _dbContext.AncillaryServiceDefinitions
                .Where(row => row.OwnerAirlineId == definition.OwnerAirlineId && row.ServiceDefinitionRef == definition.ServiceDefinitionRef)
                .Select(row => row.Id);
            var mustCheckAvailability = await _dbContext.AncillaryProvisions
                .AsNoTracking()
                .AnyAsync(
                    provision => versions.Contains(provision.ServiceDefinitionId)
                                 && provision.Status == ProvisionStatus.Active
                                 && provision.Availability.MustCheckAvailability,
                    cancellationToken);

            return new InventoryCommercialFacts(
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                definition.PricingUnit,
                providerKey,
                mustCheckAvailability);
        }
    }
}
