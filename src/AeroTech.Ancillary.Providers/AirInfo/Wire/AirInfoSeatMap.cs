namespace AeroTech.Ancillary.Providers.AirInfo.Wire
{
    public sealed class AirInfoSeatMap
    {
        public List<AirInfoDeck>? Decks { get; set; }

        public AirInfoDictionaries? Dictionaries { get; set; }
    }

    public sealed class AirInfoDeck
    {
        public string? DeckType { get; set; }

        public AirInfoDeckConfiguration? DeckConfiguration { get; set; }

        public List<AirInfoCabin>? Cabins { get; set; }
    }

    public sealed class AirInfoDeckConfiguration
    {
        public int Width { get; set; }

        public int Length { get; set; }

        public int StartSeatRow { get; set; }

        public int EndSeatRow { get; set; }

        public int StartWingsX { get; set; }

        public int EndWingsX { get; set; }

        public int StartWingsRow { get; set; }

        public int EndWingsRow { get; set; }

        public List<int>? ExitRowsX { get; set; }
    }

    public sealed class AirInfoCabin
    {
        public string? CabinClass { get; set; }

        public int CabinClassId { get; set; }

        public int CabinStartSeatRow { get; set; }

        public int CabinEndSeatRow { get; set; }

        public List<AirInfoCabinItem>? Items { get; set; }
    }

    public sealed class AirInfoCabinItem
    {
        public string? Number { get; set; }

        public string? Code { get; set; }

        public int Column { get; set; }

        public int Row { get; set; }

        public string? Title { get; set; }

        public List<string>? CharacteristicsCodes { get; set; }

        public List<string>? Conditions { get; set; }

        public AirInfoCoordinates? Coordinates { get; set; }
    }

    public sealed class AirInfoCoordinates
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Length { get; set; }
    }

    public sealed class AirInfoDictionaries
    {
        public Dictionary<string, string>? Facilities { get; set; }

        public Dictionary<string, string>? SeatCharacteristics { get; set; }
    }
}
