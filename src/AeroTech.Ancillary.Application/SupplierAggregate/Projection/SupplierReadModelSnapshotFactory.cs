using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Projection
{
    internal static class SupplierReadModelSnapshotFactory
    {
        public static SupplierReadModelSnapshot ToReadModelSnapshot(this Supplier supplier)
            => new(
                supplier.Id,
                supplier.OwnerAirlineId,
                supplier.Name,
                supplier.FulfillmentKind,
                supplier.FulfillmentProviderKey,
                supplier.Status,
                supplier.CreatedAt,
                supplier.RetiredAt);
    }
}
