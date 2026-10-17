using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed class BackofficeChangePetProvisionCommandHandler
        : IRequestHandler<BackofficeChangePetProvisionCommand, ProvisionResult>
    {
        private readonly IChangeAncillaryProvisionService _service;

        public BackofficeChangePetProvisionCommandHandler(IChangeAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangePetProvisionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
