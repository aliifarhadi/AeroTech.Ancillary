using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate.Backoffice
{
    public sealed class BackofficeRemoveProvisionTravelDateCommandHandler
        : IRequestHandler<BackofficeRemoveProvisionTravelDateCommand, ProvisionConditionRowResult>
    {
        private readonly IRemoveProvisionTravelDateService _service;

        public BackofficeRemoveProvisionTravelDateCommandHandler(IRemoveProvisionTravelDateService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeRemoveProvisionTravelDateCommand command, CancellationToken cancellationToken)
            => _service.RemoveAsync(command, cancellationToken);
    }
}
