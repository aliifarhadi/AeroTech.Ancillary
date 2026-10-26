namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities
{
    public sealed class AncillaryReservationUnitFlight
    {
        private AncillaryReservationUnitFlight()
        {
        }

        internal AncillaryReservationUnitFlight(long flightId) => FlightId = flightId;

        public long FlightId { get; private set; }
    }
}
