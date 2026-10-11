using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public sealed class ApiSalesScopeResolver : IApiSalesScopeResolver
    {
        private readonly IAgencySalesScopeService _agency;

        public ApiSalesScopeResolver(IAgencySalesScopeService agency) => _agency = agency;

        public Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default) => _agency.ResolveAsync(cancellationToken);
    }
}
