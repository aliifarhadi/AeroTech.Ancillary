using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow.Backoffice
{
    public sealed class BackofficeAddProvisionDayTimeWindowCommandHandler
        : IRequestHandler<BackofficeAddProvisionDayTimeWindowCommand, ProvisionRuleRowResult>
    {
        private readonly IAddProvisionDayTimeWindowService _service;

        public BackofficeAddProvisionDayTimeWindowCommandHandler(IAddProvisionDayTimeWindowService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeAddProvisionDayTimeWindowCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
