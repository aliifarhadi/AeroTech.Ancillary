using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod.Backoffice
{
    public sealed class BackofficeRemoveProvisionBlackoutPeriodCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionBlackoutPeriodCommand, ProvisionRuleRowResult>
    {
        private readonly IRemoveProvisionBlackoutPeriodService _service;

        public BackofficeRemoveProvisionBlackoutPeriodCommandHandler(IRemoveProvisionBlackoutPeriodService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeRemoveProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
