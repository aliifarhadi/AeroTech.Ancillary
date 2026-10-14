namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record AircraftSeatMap(
        IReadOnlyList<AircraftDeck> Decks,
        IReadOnlyDictionary<string, string> Facilities,
        IReadOnlyDictionary<string, string> SeatCharacteristics);

    public sealed record AircraftDeck(
        string? DeckType,
        AircraftDeckConfiguration Configuration,
        IReadOnlyList<AircraftCabin> Cabins);

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

    public sealed record AircraftCabin(
        string? CabinClass,
        int? CabinClassId,
        int StartSeatRow,
        int EndSeatRow,
        IReadOnlyList<AircraftCabinItem> Items);

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
