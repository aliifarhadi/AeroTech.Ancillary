using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod.Backoffice
{
    public sealed class BackofficeChangeProvisionSeasonalPeriodCommandHandler
        : IRequestHandler<BackofficeChangeProvisionSeasonalPeriodCommand, ProvisionConditionRowResult>
    {
        private readonly IChangeProvisionSeasonalPeriodService _service;

        public BackofficeChangeProvisionSeasonalPeriodCommandHandler(IChangeProvisionSeasonalPeriodService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeChangeProvisionSeasonalPeriodCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
