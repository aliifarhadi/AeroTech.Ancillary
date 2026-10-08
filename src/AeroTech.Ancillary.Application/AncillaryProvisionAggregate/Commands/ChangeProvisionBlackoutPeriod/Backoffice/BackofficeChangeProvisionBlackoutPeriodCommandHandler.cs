using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod.Backoffice
{
    public sealed class BackofficeChangeProvisionBlackoutPeriodCommandHandler
        : IRequestHandler<BackofficeChangeProvisionBlackoutPeriodCommand, ProvisionRuleRowResult>
    {
        private readonly IChangeProvisionBlackoutPeriodService _service;

        public BackofficeChangeProvisionBlackoutPeriodCommandHandler(IChangeProvisionBlackoutPeriodService service) => _service = service;

        public Task<ProvisionRuleRowResult> Handle(BackofficeChangeProvisionBlackoutPeriodCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
