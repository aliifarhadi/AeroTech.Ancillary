using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service
{
    public sealed class ServiceHoldAncillaryServicesCommandHandler
        : IRequestHandler<ServiceHoldAncillaryServicesCommand, AncillaryHoldReceipt>
    {
        private readonly IHoldAncillaryServicesService _service;

        public ServiceHoldAncillaryServicesCommandHandler(IHoldAncillaryServicesService service) => _service = service;

        public Task<AncillaryHoldReceipt> Handle(ServiceHoldAncillaryServicesCommand command, CancellationToken cancellationToken)
            => _service.HoldAsync(command, cancellationToken);
    }
}
