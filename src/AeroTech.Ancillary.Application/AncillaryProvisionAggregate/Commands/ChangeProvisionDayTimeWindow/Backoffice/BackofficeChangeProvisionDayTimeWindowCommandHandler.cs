using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow.Backoffice
{
    public sealed class BackofficeChangeProvisionDayTimeWindowCommandHandler
        : IRequestHandler<BackofficeChangeProvisionDayTimeWindowCommand, ProvisionRuleRowResult>
    {
        private readonly IChangeProvisionDayTimeWindowService _service;

        public BackofficeChangeProvisionDayTimeWindowCommandHandler(IChangeProvisionDayTimeWindowService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeChangeProvisionDayTimeWindowCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
