using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication.Backoffice
{
    public sealed class BackofficeChangeProvisionFlightApplicationCommandHandler
        : IRequestHandler<BackofficeChangeProvisionFlightApplicationCommand, ProvisionResult>
    {
        private readonly IChangeProvisionFlightApplicationService _service;

        public BackofficeChangeProvisionFlightApplicationCommandHandler(IChangeProvisionFlightApplicationService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionFlightApplicationCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
