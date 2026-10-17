using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed class BackofficeDefineAirportServiceProvisionCommandHandler
        : IRequestHandler<BackofficeDefineAirportServiceProvisionCommand, ProvisionResult>
    {
        private readonly IDefineAncillaryProvisionService _service;

        public BackofficeDefineAirportServiceProvisionCommandHandler(IDefineAncillaryProvisionService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeDefineAirportServiceProvisionCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
