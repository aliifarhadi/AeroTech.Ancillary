using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IGetAncillaryOffersService
    {
        Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, ShoppingFilter family, CancellationToken cancellationToken = default);

        Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default);

        Task<(AncillaryShoppingSnapshot Shopping, AncillaryOfferItem Item)> FindItemAsync(
            IAncillaryOfferItemQuery query,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default);
    }
}
