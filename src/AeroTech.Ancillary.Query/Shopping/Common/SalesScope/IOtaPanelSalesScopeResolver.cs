
namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public interface IOtaPanelSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
