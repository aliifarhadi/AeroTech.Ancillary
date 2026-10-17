using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed class BackofficeDefineAssistedTravelProvisionCommandHandler
        : IRequestHandler<BackofficeDefineAssistedTravelProvisionCommand, ProvisionResult>
    {
        private readonly IDefineAncillaryProvisionService _service;

        public BackofficeDefineAssistedTravelProvisionCommandHandler(IDefineAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeDefineAssistedTravelProvisionCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
