namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferFareComponent
    {
        public long AirFareId { get; set; }

        public string BoundId { get; set; } = string.Empty;

        public int? CabinClassId { get; set; }

        public long? RbdId { get; set; }

        public string? BookingClass { get; set; }

        public string? FareBasis { get; set; }

        public string? FareFamily { get; set; }

        public string? FareType { get; set; }
    }
}
