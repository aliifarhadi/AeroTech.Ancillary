using AeroTech.Ancillary.Domain.SupplierAggregate;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier
{
    internal static class SupplierResultFactory
    {
        public static SupplierResult ToResult(this Supplier supplier)
            => new(
                supplier.Id,
                supplier.OwnerAirlineId,
                supplier.Name,
                supplier.FulfillmentKind,
                supplier.FulfillmentProviderKey,
                supplier.Status);
    }
}
