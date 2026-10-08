using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod.Backoffice
{
    public sealed class BackofficeAddProvisionPermittedTravelPeriodCommandHandler
        : IRequestHandler<BackofficeAddProvisionPermittedTravelPeriodCommand, ProvisionRuleRowResult>
    {
        private readonly IAddProvisionPermittedTravelPeriodService _service;

        public BackofficeAddProvisionPermittedTravelPeriodCommandHandler(IAddProvisionPermittedTravelPeriodService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeAddProvisionPermittedTravelPeriodCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
