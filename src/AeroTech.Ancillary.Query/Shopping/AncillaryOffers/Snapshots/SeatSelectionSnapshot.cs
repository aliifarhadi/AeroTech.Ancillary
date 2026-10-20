using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record SeatSelectionSnapshot(string SeatMapSelectionId, FlightSeatMap Flight, SeatTravellerOffer Offer, int CurrencyId)
    {
        public bool IsIncludedSeat => Offer.Candidate is null;

        public decimal? Total => Offer.Candidate is { } candidate ? candidate.Price.RequestedQuantityTotal : decimal.Zero;
    }
}
