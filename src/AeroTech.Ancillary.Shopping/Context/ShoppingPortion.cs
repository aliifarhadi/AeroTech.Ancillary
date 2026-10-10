namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record ShoppingPortion
    {
        public required string PortionRef { get; init; }

        public required int Sequence { get; init; }

        public required IReadOnlyList<string> FlightRefs { get; init; }

        public required int OriginAirportId { get; init; }

        public required int DestinationAirportId { get; init; }

        public IReadOnlyList<int>? ViaAirportIds { get; init; }

        public string? DirectionOrJourneyRef { get; init; }
    }
}
