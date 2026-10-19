namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferEnvelope<T>
    {
        public T? Data { get; set; }

        public OfferResultError[]? Errors { get; set; }
    }
}
