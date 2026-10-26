using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold.Service
{
    public sealed class ServiceReleaseAncillaryHoldCommandHandler : IRequestHandler<ServiceReleaseAncillaryHoldCommand, AncillaryHoldReceipt>
    {
        private readonly IReleaseAncillaryHoldService _service;

        public ServiceReleaseAncillaryHoldCommandHandler(IReleaseAncillaryHoldService service) => _service = service;

        public Task<AncillaryHoldReceipt> Handle(ServiceReleaseAncillaryHoldCommand command, CancellationToken cancellationToken)
            => _service.ReleaseAsync(command, cancellationToken);
    }
}
