namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiOtherServiceDto(
        string Name,
        ApiElementaryPriceDto? Price,
        bool BookableByTraveler,
        bool BookableByItinerary,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> TravelerIds,
        ApiAncillaryDto Ancillary);
}
