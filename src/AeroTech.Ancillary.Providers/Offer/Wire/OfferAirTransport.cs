using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed class OfferAirTransport
    {
        public string BoundId { get; set; } = string.Empty;

        public string BoundOfferId { get; set; } = string.Empty;

        public OfferBoundDirection Direction { get; set; }

        public int Sequence { get; set; }

        public int OriginAirportId { get; set; }

        public int DestinationAirportId { get; set; }

        public List<OfferFlight> Flights { get; set; } = [];
    }
}
