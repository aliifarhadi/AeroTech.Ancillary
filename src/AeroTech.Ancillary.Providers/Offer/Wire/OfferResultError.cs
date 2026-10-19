namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferResultError
    {
        public int Code { get; set; }

        public string? Title { get; set; }

        public string? Detail { get; set; }
    }
}
