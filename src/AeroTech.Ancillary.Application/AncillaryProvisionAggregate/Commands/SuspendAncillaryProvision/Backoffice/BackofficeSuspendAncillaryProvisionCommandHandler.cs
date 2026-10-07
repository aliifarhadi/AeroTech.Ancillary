using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision.Backoffice
{
    public sealed class BackofficeSuspendAncillaryProvisionCommandHandler
        : IRequestHandler<BackofficeSuspendAncillaryProvisionCommand, ProvisionResult>
    {
        private readonly ISuspendAncillaryProvisionService _service;

        public BackofficeSuspendAncillaryProvisionCommandHandler(ISuspendAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeSuspendAncillaryProvisionCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
