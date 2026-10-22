using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
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
        private readonly ILogger<AncillaryHoldExpiryPoller> _logger;

        public AncillaryHoldExpiryPoller(
            IServiceScopeFactory scopeFactory,
            IOptions<AncillaryHoldOptions> options,
            ILogger<AncillaryHoldExpiryPoller> logger)
        {
            _scopeFactory = scopeFactory;
            _interval = TimeSpan.FromSeconds(options.Value.ExpiryPollIntervalSeconds);
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
            using var scope = _scopeFactory.CreateScope();
            var expired = await scope.ServiceProvider.GetRequiredService<IExpireAncillaryHoldsService>().ExpireAsync(cancellationToken);

            if (expired > 0)
                _logger.LogInformation("Ancillary hold expiry marked {Expired} hold(s) as expired.", expired);
        }
    }
}
