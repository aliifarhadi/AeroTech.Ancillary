namespace AeroTech.Ancillary.Query.Shopping.Common.SalesScope
{
    public interface IBackofficeSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default);

        Task<AncillarySalesScope> ResolveForPointsOfSaleAsync(IReadOnlyList<long> pointOfSaleIds, CancellationToken cancellationToken = default);
    }
}
