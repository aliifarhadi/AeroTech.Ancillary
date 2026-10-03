using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation.Service
{
    public sealed record ServiceReleaseServiceReservationCommand(
        long ServiceReservationId) : IRequest<ServiceReservationResult>, IReleaseServiceReservationCommand;
}
