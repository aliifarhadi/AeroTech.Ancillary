using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IAgencySalesScopeService
    {
        Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
