namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferSelectionCoverageDto(
        string Scope,
        IReadOnlyList<string> FlightRefs,
        IReadOnlyList<string> PortionRefs);
}
