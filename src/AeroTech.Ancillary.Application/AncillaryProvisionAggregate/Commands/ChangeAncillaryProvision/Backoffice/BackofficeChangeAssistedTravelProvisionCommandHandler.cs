using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed class BackofficeChangeAssistedTravelProvisionCommandHandler
        : IRequestHandler<BackofficeChangeAssistedTravelProvisionCommand, ProvisionResult>
    {
        private readonly IChangeAncillaryProvisionService _service;

        public BackofficeChangeAssistedTravelProvisionCommandHandler(IChangeAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeAssistedTravelProvisionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
