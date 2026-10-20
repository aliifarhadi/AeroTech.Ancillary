using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiSelectionBatchDataDto(string Type, string FlightOfferId, IReadOnlyList<ApiSelectionLineDto> Lines, OfferBasketDto Basket);
}
