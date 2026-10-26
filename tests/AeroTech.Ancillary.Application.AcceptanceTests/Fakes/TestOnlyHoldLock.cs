using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class TestOnlyHoldLock : IAncillaryHoldLock
{
    private readonly HashSet<long> _held = [];

    public List<long> Acquired { get; } = [];

    public bool Busy { get; set; }

    public Task<IAsyncDisposable> AcquireAsync(long orderId, CancellationToken cancellationToken = default)
    {
        lock (_held)
        {
            if (Busy || !_held.Add(orderId))
                throw ExceptionFactory.AncillaryHoldOrderIsBusy(orderId);

            Acquired.Add(orderId);
        }

        return Task.FromResult<IAsyncDisposable>(new Release(this, orderId));
    }

    private sealed class Release(TestOnlyHoldLock owner, long orderId) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            lock (owner._held)
                owner._held.Remove(orderId);

            return ValueTask.CompletedTask;
        }
    }
}
