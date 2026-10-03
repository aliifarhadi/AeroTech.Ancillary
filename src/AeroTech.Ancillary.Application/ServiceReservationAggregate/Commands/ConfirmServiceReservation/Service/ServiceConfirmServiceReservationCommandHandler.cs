using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation.Service
{
    public sealed class ServiceConfirmServiceReservationCommandHandler : IRequestHandler<ServiceConfirmServiceReservationCommand, ServiceReservationResult>
    {
        private readonly IConfirmServiceReservationService _service;

        public ServiceConfirmServiceReservationCommandHandler(IConfirmServiceReservationService service) => _service = service;

        public Task<ServiceReservationResult> Handle(ServiceConfirmServiceReservationCommand command, CancellationToken cancellationToken)
            => _service.ConfirmAsync(command, cancellationToken);
    }
}
