namespace AeroTech.Ancillary.Application.Orders
{
    public interface IOrderCommercialContextReader
    {
        Task<OrderCommercialContext?> FindAsync(long orderId, CancellationToken cancellationToken = default);
    }
}
