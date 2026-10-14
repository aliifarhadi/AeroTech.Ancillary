using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.SeatMaps;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots
{
    public sealed record SeatMapSnapshot(
        FlightOffer Offer,
        int CurrencyId,
        string CurrencyCode,
        IReadOnlyList<FlightSeatMap> Flights);

    public sealed record FlightSeatMap(
        string FlightRef,
        FlightOfferFlight Flight,
        int AircraftId,
        AircraftSeatMap Map,
        IReadOnlyList<SeatTravellerOffer> Offers)
    {
        public IReadOnlyList<SeatTravellerOffer> OffersOf(string seatNumber)
            => Offers.Where(offer => string.Equals(offer.SeatNumber, seatNumber.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public sealed record SeatTravellerOffer(
        string TravellerRef,
        string SeatNumber,
        SeatAvailabilityStatus Status,
        string? Id,
        PriceAssessment? Price);
}
