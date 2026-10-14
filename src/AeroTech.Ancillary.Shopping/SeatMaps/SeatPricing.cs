using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Shopping.SeatMaps
{
    public sealed record SeatPricing(
        string TravellerRef,
        string FlightRef,
        string SeatNumber,
        SeatAvailabilityStatus Status,
        CanonicalAncillaryOfferCandidate? Candidate);
}
