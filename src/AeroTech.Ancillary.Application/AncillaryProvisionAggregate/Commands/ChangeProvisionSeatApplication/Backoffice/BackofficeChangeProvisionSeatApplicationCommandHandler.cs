using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication.Backoffice
{
    public sealed class BackofficeChangeProvisionSeatApplicationCommandHandler
        : IRequestHandler<BackofficeChangeProvisionSeatApplicationCommand, ProvisionResult>
    {
        private readonly IChangeProvisionSeatApplicationService _service;

        public BackofficeChangeProvisionSeatApplicationCommandHandler(IChangeProvisionSeatApplicationService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionSeatApplicationCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
