using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using MediatR;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits.Service
{
    public sealed record ServiceCancelServiceReservationUnitsCommand(
        long ServiceReservationId,
        IReadOnlyList<long> UnitRefs) : IRequest<ServiceReservationResult>, ICancelServiceReservationUnitsCommand;
}
