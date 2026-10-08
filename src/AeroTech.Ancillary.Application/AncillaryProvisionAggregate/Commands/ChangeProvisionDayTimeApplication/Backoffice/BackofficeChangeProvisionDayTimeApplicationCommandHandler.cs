using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication.Backoffice
{
    public sealed class BackofficeChangeProvisionDayTimeApplicationCommandHandler
        : IRequestHandler<BackofficeChangeProvisionDayTimeApplicationCommand, ProvisionResult>
    {
        private readonly IChangeProvisionDayTimeApplicationService _service;

        public BackofficeChangeProvisionDayTimeApplicationCommandHandler(IChangeProvisionDayTimeApplicationService service) => _service = service;

        public Task<ProvisionResult> Handle(BackofficeChangeProvisionDayTimeApplicationCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
