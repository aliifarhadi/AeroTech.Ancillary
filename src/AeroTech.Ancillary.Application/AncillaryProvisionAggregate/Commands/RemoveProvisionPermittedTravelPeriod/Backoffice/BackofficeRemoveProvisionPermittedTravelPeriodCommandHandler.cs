using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod.Backoffice
{
    public sealed class BackofficeRemoveProvisionPermittedTravelPeriodCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionPermittedTravelPeriodCommand, ProvisionRuleRowResult>
    {
        private readonly IRemoveProvisionPermittedTravelPeriodService _service;

        public BackofficeRemoveProvisionPermittedTravelPeriodCommandHandler(IRemoveProvisionPermittedTravelPeriodService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeRemoveProvisionPermittedTravelPeriodCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
