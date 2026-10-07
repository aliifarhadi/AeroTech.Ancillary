using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold.Service
{
    public sealed class ServiceConfirmAncillaryHoldCommandHandler
        : IRequestHandler<ServiceConfirmAncillaryHoldCommand, AncillaryHoldResult>
    {
        private readonly IConfirmAncillaryHoldService _service;

        public ServiceConfirmAncillaryHoldCommandHandler(IConfirmAncillaryHoldService service) => _service = service;

        public Task<AncillaryHoldResult> Handle(ServiceConfirmAncillaryHoldCommand command, CancellationToken cancellationToken)
            => _service.ConfirmAsync(command, cancellationToken);
    }
}
