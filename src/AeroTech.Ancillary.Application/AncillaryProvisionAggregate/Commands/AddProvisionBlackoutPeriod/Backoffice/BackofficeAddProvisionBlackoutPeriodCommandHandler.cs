using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice
{
    public sealed class BackofficeAddProvisionBlackoutPeriodCommandHandler
        : IRequestHandler<BackofficeAddProvisionBlackoutPeriodCommand, ProvisionRuleRowResult>
    {
        private readonly IAddProvisionBlackoutPeriodService _service;

        public BackofficeAddProvisionBlackoutPeriodCommandHandler(IAddProvisionBlackoutPeriodService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeAddProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
