using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold.Service
{
    public sealed class ServiceCancelAncillaryHoldCommandHandler : IRequestHandler<ServiceCancelAncillaryHoldCommand, AncillaryHoldResult>
    {
        private readonly ICancelAncillaryHoldService _service;

        public ServiceCancelAncillaryHoldCommandHandler(ICancelAncillaryHoldService service) => _service = service;

        public Task<AncillaryHoldResult> Handle(ServiceCancelAncillaryHoldCommand command, CancellationToken cancellationToken)
            => _service.CancelAsync(command, cancellationToken);
    }
}
