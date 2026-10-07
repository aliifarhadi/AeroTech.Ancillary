using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision.Backoffice
{
    public sealed class BackofficeRetireAncillaryProvisionCommandHandler
        : IRequestHandler<BackofficeRetireAncillaryProvisionCommand, ProvisionResult>
    {
        private readonly IRetireAncillaryProvisionService _service;

        public BackofficeRetireAncillaryProvisionCommandHandler(IRetireAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeRetireAncillaryProvisionCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
