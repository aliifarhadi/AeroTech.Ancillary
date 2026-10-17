using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice
{
    public sealed class BackofficeChangeBaggageProvisionCommandHandler
        : IRequestHandler<BackofficeChangeBaggageProvisionCommand, ProvisionResult>
    {
        private readonly IChangeAncillaryProvisionService _service;

        public BackofficeChangeBaggageProvisionCommandHandler(IChangeAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeBaggageProvisionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
