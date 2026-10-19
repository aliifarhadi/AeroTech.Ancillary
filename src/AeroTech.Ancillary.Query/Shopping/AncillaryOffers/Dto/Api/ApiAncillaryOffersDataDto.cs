namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiAncillaryOffersDataDto(
        string Type,
        string FlightOfferId,
        IReadOnlyList<ApiTravelerDto> Travelers,
        IReadOnlyList<ApiSegmentDto> Segments);
}
