namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferBasketDto(
        bool AllReady,
        int CurrencyId,
        decimal? Total,
        IReadOnlyList<OfferPriceLineDto> SharedFees,
        IReadOnlyList<string> ReasonCodes);
}
