using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Framework.Core.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AeroTech.Ancillary.Consumers.Jobs
{
    public sealed class AncillaryHoldExpiryPoller : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval;
        private readonly int _batchSize;
        private readonly ILogger<AncillaryHoldExpiryPoller> _logger;

        public AncillaryHoldExpiryPoller(
            IServiceScopeFactory scopeFactory,
            IOptions<AncillaryHoldOptions> options,
            ILogger<AncillaryHoldExpiryPoller> logger)
        {
            _scopeFactory = scopeFactory;
            _interval = TimeSpan.FromSeconds(options.Value.ExpiryPollIntervalSeconds);
            _batchSize = options.Value.ExpiryBatchSize;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireAsync(stoppingToken);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(exception, "Ancillary hold expiry loop failed.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ExpireAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<long> dueOrderIds;

            using (var scope = _scopeFactory.CreateScope())
                dueOrderIds = await scope.ServiceProvider.GetRequiredService<IExpireAncillaryHoldsService>().ListDueOrderIdsAsync(_batchSize, cancellationToken);

            foreach (var orderId in dueOrderIds)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var expired = await scope.ServiceProvider.GetRequiredService<IExpireAncillaryHoldsService>().ExpireAsync(orderId, cancellationToken);

                    if (expired > 0)
                        _logger.LogInformation("Ancillary hold expiry marked {Expired} hold(s) of order {OrderId} as expired.", expired, orderId);
                }
                catch (BusinessException exception)
                {
                    _logger.LogInformation(exception, "Ancillary holds of order {OrderId} were not expired this round.", orderId);
                }
                catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(exception, "Expiring the ancillary holds of order {OrderId} failed.", orderId);
                }
            }
        }
    }
}
