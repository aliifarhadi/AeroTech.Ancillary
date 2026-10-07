using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision.Backoffice
{
    public sealed class BackofficeActivateAncillaryProvisionCommandHandler
        : IRequestHandler<BackofficeActivateAncillaryProvisionCommand, ProvisionResult>
    {
        private readonly IActivateAncillaryProvisionService _service;

        public BackofficeActivateAncillaryProvisionCommandHandler(IActivateAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeActivateAncillaryProvisionCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
