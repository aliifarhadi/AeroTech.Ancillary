using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod.Backoffice
{
    public sealed class BackofficeChangeProvisionPermittedTravelPeriodCommandHandler
        : IRequestHandler<BackofficeChangeProvisionPermittedTravelPeriodCommand, ProvisionRuleRowResult>
    {
        private readonly IChangeProvisionPermittedTravelPeriodService _service;

        public BackofficeChangeProvisionPermittedTravelPeriodCommandHandler(IChangeProvisionPermittedTravelPeriodService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeChangeProvisionPermittedTravelPeriodCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
