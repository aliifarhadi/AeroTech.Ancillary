
namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public interface IApiSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
