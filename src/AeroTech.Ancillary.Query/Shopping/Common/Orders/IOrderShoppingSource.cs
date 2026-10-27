using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public interface IOrderShoppingSource
    {
        Task<OrderShopping> LoadAsync(long? orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
