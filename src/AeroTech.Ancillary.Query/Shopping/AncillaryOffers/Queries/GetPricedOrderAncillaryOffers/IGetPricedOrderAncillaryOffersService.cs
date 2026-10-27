using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers
{
    public interface IGetPricedOrderAncillaryOffersService
    {
        Task<OrderAncillaryOffersSnapshot> ExecuteAsync(string selectionId, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
