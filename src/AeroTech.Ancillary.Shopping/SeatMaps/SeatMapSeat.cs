namespace AeroTech.Ancillary.Shopping.SeatMaps
{
    public sealed record SeatMapSeat(
        string FlightRef,
        string SeatNumber,
        int? CabinClassId,
        bool Occupied);
}
