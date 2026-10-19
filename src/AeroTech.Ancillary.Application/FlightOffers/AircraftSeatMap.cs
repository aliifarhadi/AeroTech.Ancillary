namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record AircraftSeatMap(
        IReadOnlyList<AircraftDeck> Decks,
        IReadOnlyDictionary<string, string> Facilities,
        IReadOnlyDictionary<string, string> SeatCharacteristics);
}
