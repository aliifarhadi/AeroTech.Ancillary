using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelSelectionBatchDto(string OfferId, IReadOnlyList<OtaPanelSelectionLineDto> Lines, OfferBasketDto Basket);
}
