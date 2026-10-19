namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record AircraftCabinItem(
        string? Number,
        string? Code,
        int Column,
        int Row,
        string? Title,
        IReadOnlyList<string> CharacteristicCodes,
        IReadOnlyList<string> Conditions,
        int X,
        int Y,
        int Length)
    {
        public const string SeatCode = "Seat";

        public bool IsSeat => string.Equals(Code, SeatCode, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Number);
    }
}
