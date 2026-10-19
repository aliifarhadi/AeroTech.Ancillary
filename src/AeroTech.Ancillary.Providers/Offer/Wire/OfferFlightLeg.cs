namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferFlightLeg
    {
        public int Sequence { get; set; }

        public long LegId { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public DateTimeOffset DepartureDateTime { get; set; }

        public DateTimeOffset ArrivalDateTime { get; set; }
    }
}
