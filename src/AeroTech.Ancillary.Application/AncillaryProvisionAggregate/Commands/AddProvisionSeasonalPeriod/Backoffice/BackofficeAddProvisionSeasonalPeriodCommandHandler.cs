using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod.Backoffice
{
    public sealed class BackofficeAddProvisionSeasonalPeriodCommandHandler
        : IRequestHandler<BackofficeAddProvisionSeasonalPeriodCommand, ProvisionConditionRowResult>
    {
        private readonly IAddProvisionSeasonalPeriodService _service;

        public BackofficeAddProvisionSeasonalPeriodCommandHandler(IAddProvisionSeasonalPeriodService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeAddProvisionSeasonalPeriodCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
