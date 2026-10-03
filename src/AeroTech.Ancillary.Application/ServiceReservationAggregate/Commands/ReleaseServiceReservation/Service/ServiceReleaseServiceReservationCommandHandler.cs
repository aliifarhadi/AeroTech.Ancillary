using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation.Service
{
    public sealed class ServiceReleaseServiceReservationCommandHandler : IRequestHandler<ServiceReleaseServiceReservationCommand, ServiceReservationResult>
    {
        private readonly IReleaseServiceReservationService _service;

        public ServiceReleaseServiceReservationCommandHandler(IReleaseServiceReservationService service) => _service = service;

        public Task<ServiceReservationResult> Handle(ServiceReleaseServiceReservationCommand command, CancellationToken cancellationToken)
            => _service.ReleaseAsync(command, cancellationToken);
    }
}
