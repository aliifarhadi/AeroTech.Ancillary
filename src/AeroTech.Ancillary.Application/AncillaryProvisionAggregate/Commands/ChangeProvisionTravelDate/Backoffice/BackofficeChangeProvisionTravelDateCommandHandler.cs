using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice
{
    public sealed class BackofficeChangeProvisionTravelDateCommandHandler
        : IRequestHandler<BackofficeChangeProvisionTravelDateCommand, ProvisionConditionRowResult>
    {
        private readonly IChangeProvisionTravelDateService _service;

        public BackofficeChangeProvisionTravelDateCommandHandler(IChangeProvisionTravelDateService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeChangeProvisionTravelDateCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
