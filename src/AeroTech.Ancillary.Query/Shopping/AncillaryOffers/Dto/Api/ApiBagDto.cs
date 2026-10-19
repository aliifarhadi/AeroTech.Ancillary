namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiBagDto(
        int? Quantity,
        decimal? Weight,
        string? WeightUnit,
        string Name,
        ApiElementaryPriceDto? Price,
        bool BookableByItinerary,
        IReadOnlyList<string> SegmentIds,
        IReadOnlyList<string> TravelerIds,
        ApiAncillaryDto Ancillary);
}
