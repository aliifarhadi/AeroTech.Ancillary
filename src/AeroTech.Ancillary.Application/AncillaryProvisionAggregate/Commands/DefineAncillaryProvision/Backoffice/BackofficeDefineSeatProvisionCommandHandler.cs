using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed class BackofficeDefineSeatProvisionCommandHandler
        : IRequestHandler<BackofficeDefineSeatProvisionCommand, ProvisionResult>
    {
        private readonly IDefineAncillaryProvisionService _service;

        public BackofficeDefineSeatProvisionCommandHandler(IDefineAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeDefineSeatProvisionCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
