namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record AircraftCabin(
        string? CabinClass,
        int? CabinClassId,
        int StartSeatRow,
        int EndSeatRow,
        IReadOnlyList<AircraftCabinItem> Items);
}
