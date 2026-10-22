using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.Extensions.Options;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed class AncillaryHoldLock : IAncillaryHoldLock
    {
        private readonly IDistributedLock _distributedLock;
        private readonly TimeSpan _expiry;

        public AncillaryHoldLock(IDistributedLock distributedLock, IOptions<AncillaryHoldOptions> options)
        {
            _distributedLock = distributedLock;
            _expiry = TimeSpan.FromSeconds(options.Value.LockExpirySeconds);
        }

        public async Task<IAsyncDisposable> AcquireAsync(long orderId, CancellationToken cancellationToken = default)
            => await _distributedLock.AcquireAsync($"ancillary-hold:{orderId}", _expiry, cancellationToken)
               ?? throw ExceptionFactory.AncillaryHoldOrderIsBusy(orderId);
    }
}
