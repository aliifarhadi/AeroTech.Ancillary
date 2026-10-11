using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelSalesScopeResolver : IOtaPanelSalesScopeResolver
    {
        private readonly IAgencySalesScopeService _agency;

        public OtaPanelSalesScopeResolver(IAgencySalesScopeService agency) => _agency = agency;

        public Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default) => _agency.ResolveAsync(cancellationToken);
    }
}
