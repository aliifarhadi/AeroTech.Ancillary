using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public interface IApiSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
