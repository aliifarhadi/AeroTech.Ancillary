using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation.Service
{
    public sealed class ServiceReserveServiceReservationCommandHandler : IRequestHandler<ServiceReserveServiceReservationCommand, ServiceReservationResult>
    {
        private readonly IReserveServiceReservationService _service;

        public ServiceReserveServiceReservationCommandHandler(IReserveServiceReservationService service) => _service = service;

        public Task<ServiceReservationResult> Handle(ServiceReserveServiceReservationCommand command, CancellationToken cancellationToken)
            => _service.ReserveAsync(command, cancellationToken);
    }
}
