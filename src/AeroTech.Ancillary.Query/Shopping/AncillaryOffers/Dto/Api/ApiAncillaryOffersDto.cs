namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiAncillaryOffersDto(
        ApiMetaDto Meta,
        ApiAncillaryOffersDataDto Data,
        ApiIncludedDto Included);
}
