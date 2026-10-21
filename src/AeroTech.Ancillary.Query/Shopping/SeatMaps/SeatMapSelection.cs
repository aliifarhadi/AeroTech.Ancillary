namespace AeroTech.Ancillary.Query.Shopping.SeatMaps
{
    public sealed record SeatMapSelection(string TravellerRef, long FlightId, long PricingId, string SeatNumber);
}
