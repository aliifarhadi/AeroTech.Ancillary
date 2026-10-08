using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice
{
    public sealed class BackofficePublishAncillaryProvisionCommandHandler
        : IRequestHandler<BackofficePublishAncillaryProvisionCommand, ProvisionResult>
    {
        private readonly IPublishAncillaryProvisionService _service;

        public BackofficePublishAncillaryProvisionCommandHandler(IPublishAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficePublishAncillaryProvisionCommand command, CancellationToken cancellationToken)
            => _service.PublishAsync(command, cancellationToken);
    }
}
