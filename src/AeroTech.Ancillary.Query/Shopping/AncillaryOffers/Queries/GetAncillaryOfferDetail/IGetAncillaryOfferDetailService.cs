using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail
{
    public interface IGetAncillaryOfferDetailService
    {
        Task<AncillaryOfferDetailSnapshot> ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<AncillaryOfferDetailSnapshot> ExecuteAsync(IAncillaryOfferItemQuery query, AncillarySalesScope scope, ShoppingFilter family, CancellationToken cancellationToken = default);
    }
}
