namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record AircraftDeck(
        string? DeckType,
        AircraftDeckConfiguration Configuration,
        IReadOnlyList<AircraftCabin> Cabins);
}
