
namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public interface IBackofficeSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default);
    }
}
