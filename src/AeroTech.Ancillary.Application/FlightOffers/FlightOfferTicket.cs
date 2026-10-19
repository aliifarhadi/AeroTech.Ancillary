using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferTicket(
        string TravellerRef,
        int TravellerIndex,
        PassengerTypeCode PassengerTypeCode,
        IReadOnlyList<FlightOfferCoupon> Coupons);
}
