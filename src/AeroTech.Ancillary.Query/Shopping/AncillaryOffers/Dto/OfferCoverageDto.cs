namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record OfferCoverageDto(
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<string> PortionRefs,
        IReadOnlyList<string> FlightRefs);
}
