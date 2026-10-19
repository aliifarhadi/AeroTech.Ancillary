namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferFlight
    {
        public int Sequence { get; set; }

        public long FlightCapacityId { get; set; }

        public long FlightId { get; set; }

        public int FlightVersion { get; set; }

        public string? FlightNumber { get; set; }

        public int OriginAirportId { get; set; }

        public int? OriginAirportTerminalId { get; set; }

        public int DestinationAirportId { get; set; }

        public int? DestinationAirportTerminalId { get; set; }

        public int OperatingAirlineId { get; set; }

        public int MarketingAirlineId { get; set; }

        public DateTimeOffset DepartureDateTime { get; set; }

        public DateTimeOffset ArrivalDateTime { get; set; }

        public int? AircraftId { get; set; }

        public int? CabinClassId { get; set; }

        public long? RbdId { get; set; }

        public string? BookingClass { get; set; }

        public List<OfferFlightLeg> Legs { get; set; } = [];
    }
}
