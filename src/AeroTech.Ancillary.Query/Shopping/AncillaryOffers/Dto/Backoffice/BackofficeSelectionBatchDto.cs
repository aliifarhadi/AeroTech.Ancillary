using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeSelectionBatchDto(string OfferId, IReadOnlyList<BackofficeSelectionLineDto> Lines, OfferBasketDto Basket);
}
