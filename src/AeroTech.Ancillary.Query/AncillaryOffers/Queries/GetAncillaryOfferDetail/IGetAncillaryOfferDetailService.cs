using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail
{
    public interface IGetAncillaryOfferDetailService
    {
        Task<AncillaryOfferDetailSnapshot> ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<AncillaryOfferDetailSnapshot> ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, ShoppingFilter family, CancellationToken cancellationToken = default);
    }
}
