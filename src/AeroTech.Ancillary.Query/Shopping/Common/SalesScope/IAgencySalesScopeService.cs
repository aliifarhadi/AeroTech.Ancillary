
namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public interface IAgencySalesScopeService
    {
        Task<AncillarySalesScope> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
