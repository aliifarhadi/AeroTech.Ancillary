using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Query.SupplierAggregate.Models;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Synchronizer.SupplierAggregate
{
    public sealed class SupplierQueryDbSynchronizer : ISupplierQueryDbSynchronizer
    {
        private readonly AncillaryQueryDbContext _dbContext;
        private readonly IClock _clock;

        public SupplierQueryDbSynchronizer(AncillaryQueryDbContext dbContext, IClock clock)
        {
            _dbContext = dbContext;
            _clock = clock;
        }

        public async Task ProjectAsync(SupplierReadModelSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(row => row.Id == snapshot.SupplierId, cancellationToken);

            if (supplier is null)
            {
                supplier = new SupplierReadModel { Id = snapshot.SupplierId };
                _dbContext.Suppliers.Add(supplier);
            }

            supplier.OwnerAirlineId = snapshot.OwnerAirlineId;
            supplier.Name = snapshot.Name;
            supplier.FulfillmentKind = snapshot.FulfillmentKind;
            supplier.FulfillmentProviderKey = snapshot.FulfillmentProviderKey;
            supplier.Status = snapshot.Status;
            supplier.CreatedAt = snapshot.CreatedAt;
            supplier.RetiredAt = snapshot.RetiredAt;
            supplier.LastUpdateTime = _clock.GetDateTime();
        }
    }
}
