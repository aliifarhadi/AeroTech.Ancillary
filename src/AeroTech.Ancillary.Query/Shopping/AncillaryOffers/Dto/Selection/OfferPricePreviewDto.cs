namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferPricePreviewDto(
        int CurrencyId,
        decimal Amount,
        bool IsComplete);
}
