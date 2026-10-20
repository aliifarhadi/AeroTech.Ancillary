using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public static class OfferSelectionBatchMapper
    {
        public static OfferSeatSelectionDto Seat(SelectionLineSnapshot line, Func<string, string> travellerId, Func<FlightSeatMap, string> segmentId)
        {
            var seat = line.Seat!;
            var paid = seat.Offer.Candidate;

            return new OfferSeatSelectionDto(
                seat.SeatMapSelectionId,
                seat.Offer.SeatNumber,
                travellerId(seat.Offer.TravellerRef),
                segmentId(seat.Flight),
                seat.Flight.Flight.FlightId,
                seat.Flight.Flight.FlightCapacityId,
                seat.IsIncludedSeat,
                paid?.ServiceDefinitionRef,
                paid?.Price.CurrencyId ?? seat.CurrencyId,
                seat.Total,
                paid is null ? null : OfferSelectionResultsMapper.ExactPrice(paid.Price, line.CanPurchase),
                paid?.Availability.RequiresCheckAtReserve ?? false,
                paid?.ReasonCodes ?? []);
        }

        public static OfferBasketDto Basket(SelectionBasket basket)
            => new(basket.AllReady, basket.CurrencyId, basket.Total, basket.SharedFees.Select(OfferSelectionResultsMapper.Line).ToList(), basket.ReasonCodes);
    }
}
