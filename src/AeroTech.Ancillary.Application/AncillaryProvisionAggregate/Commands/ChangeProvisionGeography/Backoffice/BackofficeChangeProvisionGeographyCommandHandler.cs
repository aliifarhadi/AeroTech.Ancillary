using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography.Backoffice
{
    public sealed class BackofficeChangeProvisionGeographyCommandHandler
        : IRequestHandler<BackofficeChangeProvisionGeographyCommand, ProvisionResult>
    {
        private readonly IChangeProvisionGeographyService _service;

        public BackofficeChangeProvisionGeographyCommandHandler(IChangeProvisionGeographyService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionGeographyCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
