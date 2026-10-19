namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferCoupon
    {
        public string BoundId { get; set; } = string.Empty;

        public long FlightId { get; set; }

        public int BaggagePieces { get; set; }

        public decimal BaggageWeight { get; set; }

        public string? BaggageUnit { get; set; }

        public int CabinBaggagePieces { get; set; }

        public decimal CabinBaggageWeight { get; set; }

        public string? CabinBaggageUnit { get; set; }

        public bool IsRefundable { get; set; }

        public bool IsChangeable { get; set; }

        public bool IsUpgradable { get; set; }

        public List<OfferPricingLine> Pricings { get; set; } = [];
    }
}
