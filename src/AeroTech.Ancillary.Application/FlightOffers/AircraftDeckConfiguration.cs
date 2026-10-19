namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record AircraftDeckConfiguration(
        int Width,
        int Length,
        int StartSeatRow,
        int EndSeatRow,
        int StartWingsX,
        int EndWingsX,
        int StartWingsRow,
        int EndWingsRow,
        IReadOnlyList<int> ExitRowsX);
}
