namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public sealed record PricedSeat(string TravellerRef, string FlightRef, long FlightId, long FlightCapacityId, string SeatNumber);
}
