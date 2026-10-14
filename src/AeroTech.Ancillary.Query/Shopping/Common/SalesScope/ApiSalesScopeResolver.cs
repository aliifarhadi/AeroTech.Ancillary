
namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public sealed class ApiSalesScopeResolver : IApiSalesScopeResolver
    {
        private readonly IAgencySalesScopeService _agency;

        public ApiSalesScopeResolver(IAgencySalesScopeService agency) => _agency = agency;

        public Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default) => _agency.ResolveAsync(cancellationToken);
    }
}
