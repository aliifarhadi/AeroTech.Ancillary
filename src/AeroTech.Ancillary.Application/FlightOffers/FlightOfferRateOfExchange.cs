namespace AeroTech.Ancillary.Application.FlightOffers
{
    public sealed record FlightOfferRateOfExchange(
        string RateOfExchangePeriodId,
        int FromCurrencyId,
        int ToCurrencyId,
        decimal Rate,
        int DecimalPlaces,
        float RoundingFactor);
}
