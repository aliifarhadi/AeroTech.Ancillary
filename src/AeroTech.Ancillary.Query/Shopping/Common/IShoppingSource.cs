using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public interface IShoppingSource
    {
        Task<FlightOfferShopping> LoadAsync(string? offerId, long? orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
