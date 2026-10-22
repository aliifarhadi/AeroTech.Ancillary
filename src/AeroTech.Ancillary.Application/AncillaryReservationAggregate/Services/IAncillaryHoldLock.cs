namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public interface IAncillaryHoldLock
    {
        Task<IAsyncDisposable> AcquireAsync(long orderId, CancellationToken cancellationToken = default);
    }
}
