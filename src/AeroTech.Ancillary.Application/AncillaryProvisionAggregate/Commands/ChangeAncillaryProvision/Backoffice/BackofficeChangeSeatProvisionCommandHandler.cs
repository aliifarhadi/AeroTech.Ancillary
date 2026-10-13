using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed class BackofficeChangeSeatProvisionCommandHandler
        : IRequestHandler<BackofficeChangeSeatProvisionCommand, ProvisionResult>
    {
        private readonly IChangeAncillaryProvisionService _service;

        public BackofficeChangeSeatProvisionCommandHandler(IChangeAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeSeatProvisionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
