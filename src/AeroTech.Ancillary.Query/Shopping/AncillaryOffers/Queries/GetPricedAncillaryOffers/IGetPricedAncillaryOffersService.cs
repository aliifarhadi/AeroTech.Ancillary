using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers
{
    public interface IGetPricedAncillaryOffersService
    {
        Task<AncillaryOffersSnapshot> ExecuteAsync(string selectionId, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
