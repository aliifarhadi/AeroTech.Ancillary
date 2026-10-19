namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferFxEvidenceDto(
        string RateOfExchangeTableId,
        int FromCurrencyId,
        int ToCurrencyId,
        decimal Rate,
        int DecimalPlaces);
}
