namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferTicket
    {
        public string TravellerRef { get; set; } = string.Empty;

        public int TravellerIndex { get; set; }

        public string PassengerTypeCode { get; set; } = string.Empty;

        public List<OfferCoupon> Coupons { get; set; } = [];
    }
}
