using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto
{
    public sealed record OfferJourneyDto(IReadOnlyList<OfferPortionDto> Portions);

    public sealed record OfferPortionDto(
        string PortionRef,
        int Sequence,
        EnumValueDto? Direction,
        int OriginAirportId,
        int DestinationAirportId,
        IReadOnlyList<OfferFlightDto> Flights);

    public sealed record OfferFlightDto(
        string FlightRef,
        long FlightId,
        int FlightVersion,
        string? FlightNumber,
        int MarketingAirlineId,
        int OperatingAirlineId,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt,
        int? CabinClassId,
        long? RbdId,
        int? AircraftId,
        IReadOnlyList<OfferFlightLegDto> Legs);

    public sealed record OfferFlightLegDto(
        int Sequence,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt);

    public sealed record OfferTravellerDto(
        string TravellerRef,
        int TravellerIndex,
        EnumValueDto PassengerTypeCode,
        IReadOnlyList<OfferIncludedBaggageDto> IncludedBaggageByFlight,
        IReadOnlyList<OfferFareDto> FareByFlight);

    public sealed record OfferIncludedBaggageDto(
        string FlightRef,
        int? CheckedPieces,
        decimal? CheckedWeight,
        EnumValueDto? CheckedWeightUnit,
        int? CabinPieces,
        decimal? CabinWeight,
        EnumValueDto? CabinWeightUnit);

    public sealed record OfferFareDto(
        string FlightRef,
        long? AirFareId,
        string? FareBasis,
        string? FareFamilyText,
        long? FareFamilyId,
        int? CabinClassId,
        long? RbdId);
}
