using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IGetAncillaryOffersService
    {
        Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default);
    }
}
