using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation.Service
{
    public sealed record ServiceConfirmServiceReservationCommand(
        long ServiceReservationId) : IRequest<ServiceReservationResult>, IConfirmServiceReservationCommand;
}
