namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class FlightOfferDetailResponse
    {
        public string OfferId { get; set; } = string.Empty;

        public DateTimeOffset PricedAt { get; set; }

        public DateTimeOffset? LastTicketingDate { get; set; }

        public int CurrencyId { get; set; }

        public string? CurrencyCode { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal ChargeAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OfferAirTransport> AirTransports { get; set; } = [];

        public List<OfferPricingUnit> PricingUnits { get; set; } = [];

        public List<OfferTicket> Tickets { get; set; } = [];

        public List<OfferPricingLine> OrderCharges { get; set; } = [];

        public List<OfferRateOfExchange> RatesOfExchange { get; set; } = [];
    }
}
