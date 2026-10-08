using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod.Backoffice
{
    public sealed class BackofficeRemoveProvisionSeasonalPeriodCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionSeasonalPeriodCommand, ProvisionConditionRowResult>
    {
        private readonly IRemoveProvisionSeasonalPeriodService _service;

        public BackofficeRemoveProvisionSeasonalPeriodCommandHandler(IRemoveProvisionSeasonalPeriodService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeRemoveProvisionSeasonalPeriodCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
