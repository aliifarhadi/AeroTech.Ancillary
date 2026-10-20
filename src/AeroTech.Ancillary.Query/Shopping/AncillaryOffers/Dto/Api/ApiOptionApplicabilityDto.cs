namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiOptionApplicabilityDto(
        IReadOnlyList<string> TravelerIds,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> PortionRefs);
}
