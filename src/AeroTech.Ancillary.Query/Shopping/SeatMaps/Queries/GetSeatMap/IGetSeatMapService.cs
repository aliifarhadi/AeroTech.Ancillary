using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap
{
    public interface IGetSeatMapService
    {
        Task<SeatMapSnapshot> ExecuteAsync(ISeatMapQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<SeatMapSnapshot> BuildAsync(FlightOfferShopping shopping, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
