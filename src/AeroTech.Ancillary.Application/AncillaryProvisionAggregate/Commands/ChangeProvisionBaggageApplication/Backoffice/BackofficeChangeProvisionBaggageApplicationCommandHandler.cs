using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication.Backoffice
{
    public sealed class BackofficeChangeProvisionBaggageApplicationCommandHandler
        : IRequestHandler<BackofficeChangeProvisionBaggageApplicationCommand, ProvisionResult>
    {
        private readonly IChangeProvisionBaggageApplicationService _service;

        public BackofficeChangeProvisionBaggageApplicationCommandHandler(IChangeProvisionBaggageApplicationService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionBaggageApplicationCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
