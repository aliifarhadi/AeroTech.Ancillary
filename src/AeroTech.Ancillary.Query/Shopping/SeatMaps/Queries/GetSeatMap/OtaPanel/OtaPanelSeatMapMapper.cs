using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.OtaPanel
{
    public static class OtaPanelSeatMapMapper
    {
        public static SeatMapsDto Map(SeatMapSnapshot snapshot)
            => SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.FlightRef);

        public static OrderSeatMapsDto Map(OrderSeatMapSnapshot snapshot) => SeatMapMapper.ForOrder(snapshot.OrderId, Map(snapshot.SeatMap));
    }
}
