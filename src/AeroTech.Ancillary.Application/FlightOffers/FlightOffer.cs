using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOffer(
        string OfferId,
        DateTimeOffset PricedAt,
        DateTimeOffset? LastTicketingDate,
        long? CustomerId,
        int CurrencyId,
        string? CurrencyCode,
        decimal BaseAmount,
        decimal ChargeAmount,
        decimal TotalAmount,
        IReadOnlyList<FlightOfferBound> Bounds,
        IReadOnlyList<FlightOfferFareComponent> FareComponents,
        IReadOnlyList<FlightOfferTicket> Tickets);

    public sealed record FlightOfferBound(
        string BoundId,
        int Sequence,
        BoundDirection? Direction,
        int OriginAirportId,
        int DestinationAirportId,
        IReadOnlyList<FlightOfferFlight> Flights);

    public sealed record FlightOfferFlight(
        int Sequence,
        long FlightId,
        int FlightVersion,
        string? FlightNumber,
        int OriginAirportId,
        int? OriginAirportTerminalId,
        int DestinationAirportId,
        int? DestinationAirportTerminalId,
        int OperatingAirlineId,
        int MarketingAirlineId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt,
        int? AircraftId,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        IReadOnlyList<FlightOfferLeg> Legs);

    public sealed record FlightOfferLeg(
        int Sequence,
        long LegId,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt);

    public sealed record FlightOfferFareComponent(
        long AirFareId,
        string BoundId,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        string? FareBasis,
        string? FareFamily,
        AirFareType? FareType);

    public sealed record FlightOfferTicket(
        string TravellerRef,
        int TravellerIndex,
        PassengerTypeCode PassengerTypeCode,
        IReadOnlyList<FlightOfferCoupon> Coupons);

    public sealed record FlightOfferCoupon(
        string BoundId,
        long FlightId,
        FlightOfferBaggage? CheckedBaggage,
        FlightOfferBaggage? CabinBaggage,
        IReadOnlyList<long> AirFareIds);

    public sealed record FlightOfferBaggage(int Pieces, decimal Weight, WeightUnit Unit);
}
