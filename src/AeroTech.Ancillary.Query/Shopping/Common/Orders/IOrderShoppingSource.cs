using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public interface IOrderShoppingSource
    {
        Task<FlightOfferShopping> LoadAsync(long orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default);
    }
}
