using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public interface IGetAncillaryOffersService
    {
        Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default);

        Task<AncillaryShoppingSnapshot> ExecuteAsync(IAncillaryOffersQuery query, AncillarySalesScope scope, ShoppingFilter family, CancellationToken cancellationToken = default);

        Task<AncillaryShoppingSnapshot> ShopAsync(FlightOfferShopping shopping, AncillarySalesScope scope, ShoppingFilter family, CancellationToken cancellationToken = default);

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
