using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Providers.Orders
{
    public sealed class NotConnectedOrderCommercialContextReader : IOrderCommercialContextReader
    {
        public Task<OrderCommercialContext?> FindAsync(long orderId, CancellationToken cancellationToken = default)
            => throw ExceptionFactory.OrderSourceUnavailable(orderId, "the Ordering commercial context endpoint is not connected");
    }
}
