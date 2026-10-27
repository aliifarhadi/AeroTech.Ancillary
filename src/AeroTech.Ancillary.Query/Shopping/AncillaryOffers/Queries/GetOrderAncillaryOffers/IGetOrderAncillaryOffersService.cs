using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers
{
    public interface IGetOrderAncillaryOffersService
    {
        Task<OrderAncillaryOffersSnapshot> ExecuteAsync(IOrderAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<OrderAncillaryOffersSnapshot> ExecuteAsync(SelectionSnapshot selection, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
