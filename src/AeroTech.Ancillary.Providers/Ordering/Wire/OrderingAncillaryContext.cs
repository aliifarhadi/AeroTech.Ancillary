namespace AeroTech.Ancillary.Providers.Ordering.Wire
{
    public sealed class OrderingAncillaryContext
    {
        public long OrderId { get; set; }

        public int CommercialVersion { get; set; }

        public DateTimeOffset? TicketedAt { get; set; }

        public int CurrencyId { get; set; }

        public long CustomerId { get; set; }

        public OrderingSales? Sales { get; set; }

        public List<OrderingTraveller> Travellers { get; set; } = [];

        public List<OrderingJourney> Journeys { get; set; } = [];

        public List<OrderingAirService> AirServices { get; set; } = [];

        public List<OrderingAncillaryService> AncillaryServices { get; set; } = [];
    }

    public sealed class OrderingSales
    {
        public long? TravelAgencyId { get; set; }
    }

    public sealed class OrderingTraveller
    {
        public long TravellerId { get; set; }

        public string PassengerTypeCode { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        public long? InfantParentTravellerId { get; set; }
    }

    public sealed class OrderingJourney
    {
        public string BoundId { get; set; } = string.Empty;

        public int Sequence { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public List<OrderingSegment> Segments { get; set; } = [];
    }

    public sealed class OrderingSegment
    {
        public long SegmentId { get; set; }

        public int Sequence { get; set; }

        public long FlightId { get; set; }

        public int FlightVersion { get; set; }

        public string? FlightNumber { get; set; }

        public int OriginAirportId { get; set; }

        public int? OriginAirportTerminalId { get; set; }

        public int DestinationAirportId { get; set; }

        public int? DestinationAirportTerminalId { get; set; }

        public int OperatingAirlineId { get; set; }

        public int MarketingAirlineId { get; set; }

        public DateTimeOffset DepartureAt { get; set; }

        public DateTimeOffset ArrivalAt { get; set; }

        public int? AircraftId { get; set; }

        public List<OrderingLeg> Legs { get; set; } = [];
    }

    public sealed class OrderingLeg
    {
        public long LegId { get; set; }

        public int Sequence { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public DateTimeOffset DepartureAt { get; set; }

        public DateTimeOffset ArrivalAt { get; set; }
    }

    public sealed class OrderingAirService
    {
        public long OrderServiceId { get; set; }

        public long TravellerId { get; set; }

        public long SegmentId { get; set; }

        public long FlightCapacityId { get; set; }

        public long? AirFareId { get; set; }

        public string? FareBasis { get; set; }

        public string? FareFamily { get; set; }

        public string? FareType { get; set; }

        public string? BookingClass { get; set; }

        public long? RbdId { get; set; }

        public long? CabinClassId { get; set; }

        public OrderingBaggage? CheckedBaggageAllowance { get; set; }

        public OrderingBaggage? CabinBaggageAllowance { get; set; }

        public bool IsRefundable { get; set; }

        public bool IsChangeable { get; set; }

        public bool IsUpgradable { get; set; }

        public bool? CouponOpenForUse { get; set; }
    }

    public sealed class OrderingBaggage
    {
        public int Pieces { get; set; }

        public decimal Weight { get; set; }

        public string? Unit { get; set; }
    }

    public sealed class OrderingAncillaryService
    {
        public long OrderServiceId { get; set; }

        public long TravellerId { get; set; }

        public string? ServiceDefinitionRef { get; set; }

        public List<long> CoveredAirServiceIds { get; set; } = [];

        public int Quantity { get; set; }

        public string? CommercialState { get; set; }
    }
}
