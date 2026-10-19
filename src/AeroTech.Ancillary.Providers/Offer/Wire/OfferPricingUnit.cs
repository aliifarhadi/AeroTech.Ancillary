namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferPricingUnit
    {
        public List<string> CoveredBoundOfferIds { get; set; } = [];

        public List<OfferFareComponent> FareComponents { get; set; } = [];
    }
}
