using System.Globalization;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice
{
    public static class BackofficeSeatMapMapper
    {
        public static SeatMapsDto Map(SeatMapSnapshot snapshot)
            => SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.Flight.FlightId.ToString(CultureInfo.InvariantCulture));

        public static OrderSeatMapsDto Map(OrderSeatMapSnapshot snapshot) => SeatMapMapper.ForOrder(snapshot.OrderId, Map(snapshot.SeatMap));
    }
}
