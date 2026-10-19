using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferFareComponent(
        long AirFareId,
        string BoundId,
        int? CabinClassId,
        long? RbdId,
        string? BookingClass,
        string? FareBasis,
        string? FareFamily,
        AirFareType? FareType);
}
