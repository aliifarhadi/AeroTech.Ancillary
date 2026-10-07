using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision.Backoffice
{
    public sealed class BackofficeReactivateAncillaryProvisionCommandHandler
        : IRequestHandler<BackofficeReactivateAncillaryProvisionCommand, ProvisionResult>
    {
        private readonly IReactivateAncillaryProvisionService _service;

        public BackofficeReactivateAncillaryProvisionCommandHandler(IReactivateAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeReactivateAncillaryProvisionCommand command, CancellationToken cancellationToken)
            => _service.ReactivateAsync(command, cancellationToken);
    }
}
