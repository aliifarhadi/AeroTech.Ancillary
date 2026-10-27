using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap
{
    public interface IGetOrderSeatMapService
    {
        Task<OrderSeatMapSnapshot> ExecuteAsync(IOrderSeatMapQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
