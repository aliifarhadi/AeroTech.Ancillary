using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferPricingLine
    {
        public OfferPricingCategory Category { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Reference { get; set; }

        public decimal Amount { get; set; }

        public int CurrencyId { get; set; }

        public bool IsPercentage { get; set; }

        public decimal EquivalentAmount { get; set; }

        public int EquivalentCurrencyId { get; set; }

        public string? RateOfExchangePeriodId { get; set; }
    }
}
