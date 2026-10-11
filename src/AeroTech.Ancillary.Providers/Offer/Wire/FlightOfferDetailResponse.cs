namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferEnvelope<T>
    {
        public T? Data { get; set; }

        public OfferResultError[]? Errors { get; set; }
    }

    public sealed class OfferResultError
    {
        public int Code { get; set; }

        public string? Title { get; set; }

        public string? Detail { get; set; }
    }

    public sealed class FlightOfferDetailResponse
    {
        public string OfferId { get; set; } = string.Empty;

        public DateTimeOffset PricedAt { get; set; }

        public DateTimeOffset? LastTicketingDate { get; set; }

        public int CurrencyId { get; set; }

        public string? CurrencyCode { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal ChargeAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OfferAirTransport> AirTransports { get; set; } = [];

        public List<OfferPricingUnit> PricingUnits { get; set; } = [];

        public List<OfferTicket> Tickets { get; set; } = [];
    }

    public sealed class OfferAirTransport
    {
        public string BoundId { get; set; } = string.Empty;

        public string BoundOfferId { get; set; } = string.Empty;

        public OfferBoundDirection Direction { get; set; }

        public int Sequence { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public List<OfferFlight> Flights { get; set; } = [];
    }

    public sealed class OfferFlight
    {
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

        public DateTimeOffset DepartureDateTime { get; set; }

        public DateTimeOffset ArrivalDateTime { get; set; }

        public int? AircraftId { get; set; }

        public int? CabinClassId { get; set; }

        public long? RbdId { get; set; }

        public string? BookingClass { get; set; }

        public List<OfferFlightLeg> Legs { get; set; } = [];
    }

    public sealed class OfferFlightLeg
    {
        public int Sequence { get; set; }

        public long LegId { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public DateTimeOffset DepartureDateTime { get; set; }

        public DateTimeOffset ArrivalDateTime { get; set; }
    }

    public sealed class OfferTicket
    {
        public string TravellerRef { get; set; } = string.Empty;

        public int TravellerIndex { get; set; }

        public string PassengerTypeCode { get; set; } = string.Empty;

        public List<OfferCoupon> Coupons { get; set; } = [];
    }

    public sealed class OfferCoupon
    {
        public string BoundId { get; set; } = string.Empty;

        public long FlightId { get; set; }

        public int BaggagePieces { get; set; }

        public decimal BaggageWeight { get; set; }

        public string? BaggageUnit { get; set; }

        public int CabinBaggagePieces { get; set; }

        public decimal CabinBaggageWeight { get; set; }

        public string? CabinBaggageUnit { get; set; }

        public List<OfferPricingLine> Pricings { get; set; } = [];
    }

    public sealed class OfferPricingUnit
    {
        public List<string> CoveredBoundOfferIds { get; set; } = [];

        public List<OfferFareComponent> FareComponents { get; set; } = [];
    }

    public sealed class OfferFareComponent
    {
        public long AirFareId { get; set; }

        public string BoundId { get; set; } = string.Empty;

        public int? CabinClassId { get; set; }

        public long? RbdId { get; set; }

        public string? BookingClass { get; set; }

        public string? FareBasis { get; set; }

        public string? FareFamily { get; set; }

        public string? FareType { get; set; }
    }

    public sealed class OfferPricingLine
    {
        public OfferPricingCategory Category { get; set; }

        public string? Reference { get; set; }
    }

    public enum OfferBoundDirection
    {
        Outbound = 1,
        Inbound = 2
    }

    public enum OfferPricingCategory
    {
        Fare = 0,
        Tax = 1,
        Fee = 2,
        Surcharge = 3
    }
}
