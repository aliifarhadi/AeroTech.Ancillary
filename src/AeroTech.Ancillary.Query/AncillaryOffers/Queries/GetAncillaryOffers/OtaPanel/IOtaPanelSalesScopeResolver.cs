using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public interface IOtaPanelSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
