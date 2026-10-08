using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow.Backoffice
{
    public sealed class BackofficeRemoveProvisionDayTimeWindowCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionDayTimeWindowCommand, ProvisionRuleRowResult>
    {
        private readonly IRemoveProvisionDayTimeWindowService _service;

        public BackofficeRemoveProvisionDayTimeWindowCommandHandler(IRemoveProvisionDayTimeWindowService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeRemoveProvisionDayTimeWindowCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
