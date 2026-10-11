using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail
{
    public interface IGetAncillaryOfferDetailService
    {
        Task<AncillaryOfferDetailSnapshot> ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
