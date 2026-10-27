using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Api
{
    public static class ApiSeatMapMapper
    {
        public static SeatMapsDto Map(SeatMapSnapshot snapshot)
        {
            var travelerIds = ApiOfferIdentifiers.TravelerIds(snapshot.Offer);
            var segmentIds = ApiOfferIdentifiers.Segments(snapshot.Offer)
                .GroupBy(segment => segment.FlightRef, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First().Id, StringComparer.Ordinal);

            return SeatMapMapper.Map(snapshot, travellerRef => travelerIds[travellerRef], flight => segmentIds[flight.FlightRef]);
        }

        public static OrderSeatMapsDto Map(OrderSeatMapSnapshot snapshot) => SeatMapMapper.ForOrder(snapshot.OrderId, Map(snapshot.SeatMap));
    }
}
