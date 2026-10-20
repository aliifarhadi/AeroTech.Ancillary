using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed class TestOrderCommercialContextReader(TestOrderStore store) : IOrderCommercialContextReader
{
    public Task<OrderCommercialContext?> FindAsync(long orderId, CancellationToken cancellationToken = default)
        => store.OrderingUnavailable
            ? throw ExceptionFactory.OrderSourceUnavailable(orderId, "the test Ordering simulator is unavailable")
            : Task.FromResult(store.ContextOf(orderId));
}
