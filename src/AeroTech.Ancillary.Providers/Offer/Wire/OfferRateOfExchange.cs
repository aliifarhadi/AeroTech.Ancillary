namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferRateOfExchange
    {
        public string RateOfExchangePeriodId { get; set; } = string.Empty;

        public int FromCurrencyId { get; set; }

        public int ToCurrencyId { get; set; }

        public decimal Rate { get; set; }

        public int DecimalPlaces { get; set; }

        public float RoundingFactor { get; set; }
    }
}
