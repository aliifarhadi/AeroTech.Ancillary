using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap
{
    public interface IGetSeatMapService
    {
        Task ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
