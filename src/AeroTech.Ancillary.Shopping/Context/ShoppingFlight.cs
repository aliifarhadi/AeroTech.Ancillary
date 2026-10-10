namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record ShoppingFlight
    {
        public required string FlightRef { get; init; }

        public required long FlightId { get; init; }

        public int? FlightVersion { get; init; }

        public string? FlightNumber { get; init; }

        public int? MarketingAirlineId { get; init; }

        public int? OperatingAirlineId { get; init; }

        public required int OriginAirportId { get; init; }

        public required int DestinationAirportId { get; init; }

        public IReadOnlyList<int>? ViaAirportIds { get; init; }

        public int? OriginCountryId { get; init; }

        public int? DestinationCountryId { get; init; }

        public int? OriginTerminalId { get; init; }

        public int? DestinationTerminalId { get; init; }

        public required DateTimeOffset DepartureAt { get; init; }

        public required DateTimeOffset ArrivalAt { get; init; }

        public string? OriginIanaTimeZoneId { get; init; }

        public int? AircraftId { get; init; }

        public int? CabinClassId { get; init; }

        public long? RbdId { get; init; }

        public string? FlightStatus { get; init; }
    }
}
