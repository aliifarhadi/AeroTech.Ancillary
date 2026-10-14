using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap
{
    public interface IGetSeatMapService
    {
        Task<SeatMapSnapshot> ExecuteAsync(ISeatMapQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
