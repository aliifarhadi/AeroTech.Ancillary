using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed class BackofficeDefinePetProvisionCommandHandler
        : IRequestHandler<BackofficeDefinePetProvisionCommand, ProvisionResult>
    {
        private readonly IDefineAncillaryProvisionService _service;

        public BackofficeDefinePetProvisionCommandHandler(IDefineAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeDefinePetProvisionCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
