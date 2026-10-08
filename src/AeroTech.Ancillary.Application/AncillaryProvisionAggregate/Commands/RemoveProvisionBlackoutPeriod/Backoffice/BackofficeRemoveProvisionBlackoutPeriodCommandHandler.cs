using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod.Backoffice
{
    public sealed class BackofficeRemoveProvisionBlackoutPeriodCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionBlackoutPeriodCommand, ProvisionConditionRowResult>
    {
        private readonly IRemoveProvisionBlackoutPeriodService _service;

        public BackofficeRemoveProvisionBlackoutPeriodCommandHandler(IRemoveProvisionBlackoutPeriodService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeRemoveProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
