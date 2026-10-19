namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferApplicabilityDto(
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<string> FlightRefs,
        IReadOnlyList<string> PortionRefs);
}
