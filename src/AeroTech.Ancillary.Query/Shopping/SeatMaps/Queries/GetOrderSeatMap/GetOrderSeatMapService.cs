using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap
{
    public sealed class GetOrderSeatMapService : IGetOrderSeatMapService
    {
        private readonly IOrderShoppingSource _orders;
        private readonly IGetSeatMapService _seatMaps;

        public GetOrderSeatMapService(IOrderShoppingSource orders, IGetSeatMapService seatMaps)
        {
            _orders = orders;
            _seatMaps = seatMaps;
        }

        public async Task<OrderSeatMapSnapshot> ExecuteAsync(IOrderSeatMapQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var order = await _orders.LoadAsync(query.OrderId, scope, cancellationToken);

            return new OrderSeatMapSnapshot(order.Order.OrderId, await _seatMaps.BuildAsync(order.Shopping, scope, cancellationToken));
        }
    }
}
