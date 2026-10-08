using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication.Backoffice
{
    public sealed class BackofficeChangeProvisionFareApplicationCommandHandler
        : IRequestHandler<BackofficeChangeProvisionFareApplicationCommand, ProvisionResult>
    {
        private readonly IChangeProvisionFareApplicationService _service;

        public BackofficeChangeProvisionFareApplicationCommandHandler(IChangeProvisionFareApplicationService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionFareApplicationCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
