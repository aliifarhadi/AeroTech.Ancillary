using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits.Service
{
    public sealed class ServiceCancelServiceReservationUnitsCommandHandler : IRequestHandler<ServiceCancelServiceReservationUnitsCommand, ServiceReservationResult>
    {
        private readonly ICancelServiceReservationUnitsService _service;

        public ServiceCancelServiceReservationUnitsCommandHandler(ICancelServiceReservationUnitsService service) => _service = service;

        public Task<ServiceReservationResult> Handle(ServiceCancelServiceReservationUnitsCommand command, CancellationToken cancellationToken)
            => _service.CancelAsync(command, cancellationToken);
    }
}
