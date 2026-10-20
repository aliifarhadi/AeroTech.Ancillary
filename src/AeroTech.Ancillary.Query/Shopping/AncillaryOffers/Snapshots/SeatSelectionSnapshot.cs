using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record SeatSelectionSnapshot(
        string SeatMapSelectionId,
        FlightSeatMap Flight,
        SeatTravellerOffer Offer,
        int CurrencyId,
        AncillarySelection Selection,
        CanonicalAncillarySelectionEvaluation? Evaluation)
    {
        public bool IsIncludedSeat => Offer.Candidate is null;

        public decimal? Total => Offer.Candidate is { } candidate ? candidate.Price.RequestedQuantityTotal : decimal.Zero;
    }
}
