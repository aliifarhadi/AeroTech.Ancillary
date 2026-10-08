using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice
{
    public sealed class BackofficeChangeProvisionTravelDateCommandHandler
        : IRequestHandler<BackofficeChangeProvisionTravelDateCommand, ProvisionResult>
    {
        private readonly IChangeProvisionTravelDateService _service;

        public BackofficeChangeProvisionTravelDateCommandHandler(IChangeProvisionTravelDateService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionTravelDateCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
