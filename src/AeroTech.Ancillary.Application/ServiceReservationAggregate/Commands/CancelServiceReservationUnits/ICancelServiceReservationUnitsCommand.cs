namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits
{
    public interface ICancelServiceReservationUnitsCommand
    {
        long ServiceReservationId { get; }

        IReadOnlyList<long> UnitRefs { get; }
    }
}
