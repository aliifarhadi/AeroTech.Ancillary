using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice
{
    public sealed class BackofficeAddProvisionBlackoutPeriodCommandHandler
        : IRequestHandler<BackofficeAddProvisionBlackoutPeriodCommand, ProvisionConditionRowResult>
    {
        private readonly IAddProvisionBlackoutPeriodService _service;

        public BackofficeAddProvisionBlackoutPeriodCommandHandler(IAddProvisionBlackoutPeriodService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeAddProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
