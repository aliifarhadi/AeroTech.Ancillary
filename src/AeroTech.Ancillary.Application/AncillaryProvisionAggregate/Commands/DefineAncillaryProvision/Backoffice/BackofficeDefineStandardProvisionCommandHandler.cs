using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed class BackofficeDefineStandardProvisionCommandHandler
        : IRequestHandler<BackofficeDefineStandardProvisionCommand, ProvisionResult>
    {
        private readonly IDefineAncillaryProvisionService _service;

        public BackofficeDefineStandardProvisionCommandHandler(IDefineAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeDefineStandardProvisionCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
