using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail
{
    public interface IGetGroupedOfferDetailService
    {
        Task<GroupedOfferDetailSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillaryProfile family,
            AncillarySelection? selection,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default);
    }
}
