using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public interface IBackofficeSalesScopeResolver
    {
        Task<AncillarySalesScope> ResolveAsync(BackofficeAncillaryOfferOptions? options, CancellationToken cancellationToken = default);
    }
}
