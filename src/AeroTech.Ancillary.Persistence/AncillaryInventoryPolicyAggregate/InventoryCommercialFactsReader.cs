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
                .Select(row => new { row.Id, row.OwnerAirlineId, row.ServiceDefinitionRef, row.PricingUnit, row.SupplierId })
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

            return new InventoryCommercialFacts(
                definition.Id,
                definition.OwnerAirlineId,
                definition.ServiceDefinitionRef,
                definition.PricingUnit,
                providerKey);
        }

        public async Task<InventoryCommercialFacts?> FindCurrentAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default)
        {
            var currentId = await _dbContext.AncillaryServiceDefinitions
                .AsNoTracking()
                .Where(row => row.OwnerAirlineId == ownerAirlineId
                              && row.ServiceDefinitionRef == serviceDefinitionRef
                              && row.Status != ServiceDefinitionStatus.Retired)
                .OrderByDescending(row => row.Status == ServiceDefinitionStatus.Active)
                .ThenByDescending(row => row.Version)
                .Select(row => (long?)row.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return currentId is { } id ? await FindAsync(id, cancellationToken) : null;
        }
    }
}
