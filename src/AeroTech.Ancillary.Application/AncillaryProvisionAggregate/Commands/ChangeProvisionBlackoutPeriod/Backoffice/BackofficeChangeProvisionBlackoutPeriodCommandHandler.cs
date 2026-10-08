using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod.Backoffice
{
    public sealed class BackofficeChangeProvisionBlackoutPeriodCommandHandler
        : IRequestHandler<BackofficeChangeProvisionBlackoutPeriodCommand, ProvisionConditionRowResult>
    {
        private readonly IChangeProvisionBlackoutPeriodService _service;

        public BackofficeChangeProvisionBlackoutPeriodCommandHandler(IChangeProvisionBlackoutPeriodService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeChangeProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
