using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed class BackofficeChangeStandardProvisionCommandHandler
        : IRequestHandler<BackofficeChangeStandardProvisionCommand, ProvisionResult>
    {
        private readonly IChangeAncillaryProvisionService _service;

        public BackofficeChangeStandardProvisionCommandHandler(IChangeAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeStandardProvisionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
