using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IGetAncillaryOffersService
    {
        Task<AncillaryOffersSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<AncillaryOffersSnapshot> ExecuteAsync(SelectionSnapshot selection, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
