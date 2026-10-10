using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record SeatCharacteristicFacts
    {
        public required string FlightRef { get; init; }

        public required string SeatNumber { get; init; }

        public required IReadOnlyList<string> CharacteristicCodes { get; init; }

        public required FactEvidence SourceCompleteness { get; init; }

        public required string SourceAuthority { get; init; }
    }
}
