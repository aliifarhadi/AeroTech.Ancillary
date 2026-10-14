namespace AeroTech.Ancillary.Providers.FlightFlow.Wire
{
    public sealed class FlightFlowOccupiedSeats
    {
        public long FlightId { get; set; }

        public List<FlightFlowOccupiedSeat>? Seats { get; set; }
    }

    public sealed class FlightFlowOccupiedSeat
    {
        public string? Seat { get; set; }
    }
}
