
namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed class OtaPanelSalesScopeResolver : IOtaPanelSalesScopeResolver
    {
        private readonly IAgencySalesScopeService _agency;

        public OtaPanelSalesScopeResolver(IAgencySalesScopeService agency) => _agency = agency;

        public Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default) => _agency.ResolveAsync(cancellationToken);
    }
}
